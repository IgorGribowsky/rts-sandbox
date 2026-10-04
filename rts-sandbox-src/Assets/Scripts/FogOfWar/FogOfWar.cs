using Assets.Scripts;
using Assets.Scripts.Infrastructure.Enums;
using RtsSandbox.Rules;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The fog of war (M-027, T-071.1): black where the player has never looked,
/// grey where the player looked before, clear where the player's side sees now.
///
/// Sight is counted on a grid, not with physics, and not every frame: a few
/// times a second every unit of the player and the allies lights a circle of
/// its SightRange. The grid goes into a small texture, darkness in alpha.
///
/// Every frame the video card turns that texture into a smooth one four
/// times larger: it slides from the previous pass to the newest, so the edge
/// does not jump, and blurs it, so the edge is a curve and not a staircase of
/// cells. The camera darkens the world by the smooth texture
/// (<see cref="FogOfWarEffect"/>), the minimap lays it over the map.
///
/// The clouds drifting over the grey are a small tiling noise texture made
/// once at start, not particles: one picture read per layer, cheap enough
/// for a phone.
///
/// Switched on and off by GameController.FogOfWarEnabled, also in play mode.
/// Off, nothing is counted and nothing is drawn; what was explored is kept.
///
/// What the player is shown of the other teams' units is decided after every
/// pass by <see cref="FogUnitSight"/> (T-071.3).
///
/// Trees block sight (T-071.2): every cell a trunk touches is a blocker in
/// the grid, rebuilt only when a tree comes or goes (cut down).
/// </summary>
public class FogOfWar : MonoBehaviour
{
    // Darkness written into the alpha: black, grey, clear.
    private const float UnexploredDarkness = 1f;
    private const float ExploredDarkness = 0.5f;

    // The smooth texture is this many times larger than the grid.
    private const int Upscale = 4;
    private const int NoiseSize = 256;

    private const int ComposePass = 1;
    private const int BlurPass = 2;

    private static readonly int FogTexId = Shader.PropertyToID("_FogTex");
    private static readonly int FogPrevTexId = Shader.PropertyToID("_FogPrevTex");
    private static readonly int FogBlendId = Shader.PropertyToID("_FogBlend");
    private static readonly int BlurStepId = Shader.PropertyToID("_BlurStep");

    [Header("Grid")]
    [Tooltip("Side of one fog cell, metres. Smaller is a truer edge and a dearer pass. Read at start.")]
    [Min(0.25f)]
    public float CellSize = 1f;

    [Tooltip("How far the fog reaches past the map rectangle, metres: units can walk a little beyond it. Read at start.")]
    [Min(0f)]
    public float EdgeMargin = 10f;

    [Tooltip("Seconds between two passes of sight.")]
    [Min(0.02f)]
    public float UpdateInterval = 0.15f;

    [Header("Edge")]
    [Tooltip("Over how many metres at the end of the sight radius the view fades out.")]
    [Min(0f)]
    public float EdgeFeather = 3f;

    [Tooltip("Blur of the fog, metres. Rounds off the cells.")]
    [Range(0f, 6f)]
    public float EdgeSoftness = 2.5f;

    [Tooltip("How far the edge wavers like a cloud, metres. Zero is a clean circle.")]
    [Range(0f, 4f)]
    public float EdgeWobble = 1f;

    [Header("Look")]
    public Shader FogShader;

    [Tooltip("Where nobody has looked yet.")]
    public Color UnexploredColor = Color.black;

    [Tooltip("How much darker the explored ground is.")]
    [Range(0f, 1f)]
    public float ExploredDarkening = 0.45f;

    [Tooltip("How much colour the explored ground loses.")]
    [Range(0f, 1f)]
    public float ExploredDesaturation = 0.7f;

    [Tooltip("Multiplied over the explored ground: a cold tint reads as fog.")]
    public Color ExploredTint = new Color(0.82f, 0.88f, 1f);

    [Header("Clouds")]
    [Tooltip("Colour of the mist drifting over the explored ground.")]
    public Color CloudColor = new Color(0.62f, 0.67f, 0.75f);

    [Tooltip("How thick the mist is. Zero turns it off.")]
    [Range(0f, 1f)]
    public float CloudStrength = 0.35f;

    [Tooltip("Size of the cloud patches, metres.")]
    [Min(1f)]
    public float CloudScale = 24f;

    [Tooltip("How fast the clouds drift, metres per second.")]
    public float CloudSpeed = 0.8f;

    [Header("Measured")]
    [Tooltip("What the last pass of sight cost, milliseconds. Read only.")]
    public float LastPassMilliseconds;

    private GameController _game;
    private FogGrid _grid;
    private Rect _area;
    private Color32[] _pixels;
    private Texture2D _current;
    private Texture2D _previous;
    private RenderTexture _smooth;
    private RenderTexture _blurTemp;
    private Texture2D _noise;
    private Material _composeMaterial;
    private int _treesVersion = -1;
    private float _passTime;
    private float _timer;
    private bool _wasOn;
    private readonly HashSet<int> _sharedTeams = new HashSet<int>();
    private FogOfWarEffect _effect;
    private FogUnitSight _unitSight;
    private PlayerEventController _playerEvents;
    private readonly System.Diagnostics.Stopwatch _stopwatch = new System.Diagnostics.Stopwatch();

    /// <summary>The fog is on: counted and drawn.</summary>
    public bool IsOn => _game != null && _game.FogOfWarEnabled;

    /// <summary>
    /// The fog as it is drawn this frame: white, alpha is the darkness, smooth.
    /// Covers <see cref="Area"/>, bottom row is the smallest Z.
    /// </summary>
    public RenderTexture SmoothTexture => _smooth;

    /// <summary>Tiling noise for the clouds and the wobble of the edge.</summary>
    public Texture2D NoiseTexture => _noise;

    /// <summary>The ground the fog covers, x and z in metres: the map plus the margin.</summary>
    public Rect Area => _area;

    /// <summary>Destroyed buildings the player still remembers, for the minimap.</summary>
    public IReadOnlyList<FogUnitSight.Ghost> Ghosts => _unitSight != null
        ? _unitSight.Ghosts
        : (IReadOnlyList<FogUnitSight.Ghost>)System.Array.Empty<FogUnitSight.Ghost>();

    /// <summary>Somebody went into the fog or came out of it.</summary>
    public event System.Action SightsChanged;

    /// <summary>
    /// The player may see, click and select this unit. True for anything that
    /// is not a registered unit, and for everything while there is no fog.
    /// </summary>
    public static bool IsSeen(GameObject unit)
    {
        var record = UnitRegistry.Of(unit);
        return record == null || record.Sight == FogSight.Visible;
    }

    /// <summary>
    /// The player knows this unit is there: seen now, or a building
    /// remembered. A right click may aim at it, a selection may not.
    /// </summary>
    public static bool IsKnown(GameObject unit)
    {
        var record = UnitRegistry.Of(unit);
        return record == null || record.Sight != FogSight.Hidden;
    }

    /// <summary>
    /// May a unit of this team pick the candidate as a target on its own. The
    /// fog is the player's: it binds the player's team only, the computer's
    /// teams see everything as before.
    /// </summary>
    public static bool CanTarget(int attackerTeamId, UnitRecord candidate)
    {
        var fog = GameServices.FogOfWar;
        return fog == null || fog._unitSight == null || attackerTeamId != fog._unitSight.PlayerTeamId
            || candidate.Sight == FogSight.Visible;
    }

    /// <summary>Is the point in a cell somebody on the player's side sees right now.</summary>
    public bool IsVisibleAt(Vector3 position)
    {
        var x = Mathf.FloorToInt((position.x - _area.xMin) / CellSize);
        var y = Mathf.FloorToInt((position.z - _area.yMin) / CellSize);
        return x >= 0 && y >= 0 && x < _grid.Width && y < _grid.Height
            && _grid[x, y] == FogState.Visible;
    }

    private void Awake()
    {
        GameServices.FogOfWar = this;
        _game = GetComponent<GameController>();

        var map = GetComponent<MapValues>();
        var a = map.LeftTopMapCornerPosition;
        var b = map.RightBottomMapCornerPosition;
        var width = Mathf.Max(1, Mathf.CeilToInt((Mathf.Abs(a.x - b.x) + EdgeMargin * 2f) / CellSize));
        var height = Mathf.Max(1, Mathf.CeilToInt((Mathf.Abs(a.z - b.z) + EdgeMargin * 2f) / CellSize));

        // Centred on the map, so the margin is the same on every side.
        var center = new Vector2((a.x + b.x) / 2f, (a.z + b.z) / 2f);
        var size = new Vector2(width * CellSize, height * CellSize);
        _area = new Rect(center - size / 2f, size);

        _grid = new FogGrid(width, height);
        _pixels = new Color32[width * height];
        for (var i = 0; i < _pixels.Length; i++)
        {
            _pixels[i] = new Color32(255, 255, 255, 255);
        }

        _current = CreateTexture("Fog Of War", width, height);
        _previous = CreateTexture("Fog Of War Previous", width, height);
        Upload(_current);
        Upload(_previous);

        _smooth = CreateRenderTexture("Fog Of War Smooth", width * Upscale, height * Upscale);
        _blurTemp = CreateRenderTexture("Fog Of War Blur", width * Upscale, height * Upscale);
        _noise = CreateNoise();

        if (FogShader != null)
        {
            _composeMaterial = new Material(FogShader) { hideFlags = HideFlags.HideAndDontSave };
        }

        Compose(1f);
    }

    private void Start()
    {
        var player = GameObject.FindGameObjectWithTag(Tag.PlayerController.ToString());
        var playerTeamId = player.GetComponent<PlayerTeamMember>().TeamId;

        // The player and the allies see together (M-009). A peaceful team is
        // an ally to everyone and must not share its sight with anybody.
        var teams = GetComponent<TeamController>();
        _sharedTeams.Add(playerTeamId);
        foreach (var teamId in teams.GetAllyTeams(playerTeamId))
        {
            if (!teams.IsPeaceful(teamId))
            {
                _sharedTeams.Add(teamId);
            }
        }

        _unitSight = new FogUnitSight(this, _sharedTeams) { PlayerTeamId = playerTeamId };
        UnitRegistry.Registered += OnUnitRegistered;
        _playerEvents = player.GetComponent<PlayerEventController>();
        _playerEvents.SelectedUnitDied += OnUnitDied;

        var cameraController = player.GetComponent<CameraController>();
        var camera = cameraController != null && cameraController.ControlledCamera != null
            ? cameraController.ControlledCamera
            : Camera.main;

        if (camera != null && FogShader != null)
        {
            _effect = camera.gameObject.AddComponent<FogOfWarEffect>();
            _effect.Init(this, new Material(FogShader) { hideFlags = HideFlags.HideAndDontSave });
            _effect.enabled = IsOn;
        }
        else
        {
            Debug.LogError("FogOfWar: no camera or no FogShader, the fog is counted but not drawn.");
        }
    }

    private void Update()
    {
        var on = IsOn;
        if (_effect != null && _effect.enabled != on)
        {
            _effect.enabled = on;
        }

        if (!on)
        {
            if (_wasOn && _unitSight != null && _unitSight.Update(false))
            {
                SightsChanged?.Invoke();
            }

            _wasOn = false;
            return;
        }

        _timer -= Time.deltaTime;
        if (!_wasOn || _timer <= 0f)
        {
            _timer = UpdateInterval;
            Pass(snap: !_wasOn);
            _wasOn = true;

            if (_unitSight != null && _unitSight.Update(true))
            {
                SightsChanged?.Invoke();
            }
        }

        Compose(Mathf.Clamp01((Time.time - _passTime) / UpdateInterval));
    }

    private void OnDestroy()
    {
        if (GameServices.FogOfWar == this)
        {
            GameServices.FogOfWar = null;
        }

        UnitRegistry.Registered -= OnUnitRegistered;
        if (_playerEvents != null)
        {
            _playerEvents.SelectedUnitDied -= OnUnitDied;
        }

        _unitSight?.Clear();

        if (_effect != null)
        {
            Destroy(_effect);
        }

        Destroy(_current);
        Destroy(_previous);
        Destroy(_noise);
        Destroy(_composeMaterial);
        ReleaseRenderTexture(_smooth);
        ReleaseRenderTexture(_blurTemp);
    }

    private void OnUnitRegistered(UnitRecord record)
    {
        // Before the first pass the grid is all black: wait for the pass.
        if (_wasOn)
        {
            _unitSight.OnRegistered(record, IsOn);
        }
    }

    /// <summary>Every death goes out under this name, not only the selected ones'.</summary>
    private void OnUnitDied(Assets.Scripts.Infrastructure.Events.DiedEventArgs args)
    {
        _unitSight.OnDied(args.Dead, IsOn && _wasOn);
    }

    /// <summary>
    /// One pass of sight. Right after the fog is switched on there is
    /// nothing worth sliding from, so both textures get the new picture.
    /// </summary>
    private void Pass(bool snap)
    {
        _stopwatch.Restart();

        if (_treesVersion != HarvestedResource.Version)
        {
            RebuildBlockers();
        }

        _grid.BeginPass();

        var feather = EdgeFeather / CellSize;
        var all = UnitRegistry.All;
        for (var i = 0; i < all.Count; i++)
        {
            var record = all[i];
            if (record.GameObject == null || record.Values == null || !_sharedTeams.Contains(record.TeamId))
            {
                continue;
            }

            var sight = record.Values.SightRange;
            if (sight <= 0f)
            {
                continue;
            }

            var position = record.Transform.position;
            _grid.Reveal(
                (position.x - _area.xMin) / CellSize,
                (position.z - _area.yMin) / CellSize,
                sight / CellSize,
                feather);
        }

        var cells = _grid.Cells;
        var light = _grid.Light;
        for (var i = 0; i < cells.Length; i++)
        {
            var darkness = cells[i] == FogState.Visible ? ExploredDarkness * (1f - light[i])
                : cells[i] == FogState.Explored ? ExploredDarkness
                : UnexploredDarkness;
            _pixels[i].a = (byte)Mathf.RoundToInt(darkness * 255f);
        }

        if (snap || SystemInfo.copyTextureSupport == UnityEngine.Rendering.CopyTextureSupport.None)
        {
            Upload(_previous);
        }
        else
        {
            Graphics.CopyTexture(_current, _previous);
        }

        Upload(_current);
        _passTime = Time.time;

        _stopwatch.Stop();
        LastPassMilliseconds = (float)_stopwatch.Elapsed.TotalMilliseconds;
    }

    /// <summary>Every cell a tree's trunk touches blocks sight.</summary>
    private void RebuildBlockers()
    {
        _treesVersion = HarvestedResource.Version;
        _grid.ClearBlockers();

        var trees = HarvestedResource.All;
        for (var i = 0; i < trees.Count; i++)
        {
            var tree = trees[i];
            var collider = tree != null ? tree.GetComponent<Collider>() : null;
            if (collider == null)
            {
                continue;
            }

            var bounds = collider.bounds;
            var xMin = Mathf.FloorToInt((bounds.min.x - _area.xMin) / CellSize);
            var xMax = Mathf.FloorToInt((bounds.max.x - _area.xMin) / CellSize);
            var yMin = Mathf.FloorToInt((bounds.min.z - _area.yMin) / CellSize);
            var yMax = Mathf.FloorToInt((bounds.max.z - _area.yMin) / CellSize);

            for (var y = yMin; y <= yMax; y++)
            {
                for (var x = xMin; x <= xMax; x++)
                {
                    _grid.SetBlocker(x, y);
                }
            }
        }
    }

    /// <summary>
    /// The smooth texture for this frame: the grid stretched with a cubic
    /// B-spline and the previous and newest pass mixed by blend, then blurred
    /// in two strokes, across and then along.
    /// </summary>
    private void Compose(float blend)
    {
        if (_composeMaterial == null)
        {
            return;
        }

        _composeMaterial.SetTexture(FogTexId, _current);
        _composeMaterial.SetTexture(FogPrevTexId, _previous);
        _composeMaterial.SetFloat(FogBlendId, blend);
        Graphics.Blit(_current, _smooth, _composeMaterial, ComposePass);

        // A Gaussian of nine taps read as five: the outer tap lies 3.23 steps out.
        var stepX = EdgeSoftness / 3.23f / _area.width;
        var stepY = EdgeSoftness / 3.23f / _area.height;

        _composeMaterial.SetVector(BlurStepId, new Vector4(stepX, 0f, 0f, 0f));
        Graphics.Blit(_smooth, _blurTemp, _composeMaterial, BlurPass);

        _composeMaterial.SetVector(BlurStepId, new Vector4(0f, stepY, 0f, 0f));
        Graphics.Blit(_blurTemp, _smooth, _composeMaterial, BlurPass);
    }

    private static Texture2D CreateTexture(string name, int width, int height)
    {
        return new Texture2D(width, height, TextureFormat.RGBA32, false)
        {
            name = name,
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave,
        };
    }

    private static RenderTexture CreateRenderTexture(string name, int width, int height)
    {
        // Half floats: in eight bits the soft gradients of the fog showed as
        // steps, and a stepped gradient reads as a low resolution.
        var format = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf)
            ? RenderTextureFormat.ARGBHalf
            : RenderTextureFormat.ARGB32;
        var texture = new RenderTexture(width, height, 0, format)
        {
            name = name,
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            useMipMap = false,
            hideFlags = HideFlags.HideAndDontSave,
        };
        texture.Create();
        return texture;
    }

    private static void ReleaseRenderTexture(RenderTexture texture)
    {
        if (texture != null)
        {
            texture.Release();
            Destroy(texture);
        }
    }

    private void Upload(Texture2D texture)
    {
        texture.SetPixels32(_pixels);
        texture.Apply(false, false);
    }

    /// <summary>
    /// Tiling gradient (Perlin) noise. Value noise was tried first: its blobs
    /// line up along the lattice and read as squares. Red is the clouds, four
    /// octaves. Green and blue are two unrelated layers that push the edge
    /// sideways: only the two broad octaves, fine ones crumble the edge.
    /// </summary>
    private static Texture2D CreateNoise()
    {
        var pixels = new Color32[NoiseSize * NoiseSize];
        for (var y = 0; y < NoiseSize; y++)
        {
            for (var x = 0; x < NoiseSize; x++)
            {
                pixels[y * NoiseSize + x] = new Color32(
                    ToByte(CloudOctaves(x, y, 0)),
                    ToByte(BroadOctaves(x, y, 101)),
                    ToByte(BroadOctaves(x, y, 211)),
                    255);
            }
        }

        var texture = new Texture2D(NoiseSize, NoiseSize, TextureFormat.RGBA32, false)
        {
            name = "Fog Of War Noise",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Repeat,
            hideFlags = HideFlags.HideAndDontSave,
        };
        texture.SetPixels32(pixels);
        texture.Apply(false, true);
        return texture;
    }

    // Perlin noise lies in about -0.7..0.7; brought into 0..1 around the middle.
    private static byte ToByte(float noise) => (byte)(Mathf.Clamp01(noise * 0.75f + 0.5f) * 255f);

    // Lattice cells across the texture are whole numbers: every octave tiles on its own.
    private static float CloudOctaves(int x, int y, int seed)
    {
        return GradientNoise(x, y, 4, seed) * 0.5f
            + GradientNoise(x, y, 8, seed + 7) * 0.28f
            + GradientNoise(x, y, 16, seed + 13) * 0.15f
            + GradientNoise(x, y, 32, seed + 19) * 0.07f;
    }

    private static float BroadOctaves(int x, int y, int seed)
    {
        return GradientNoise(x, y, 4, seed) * 0.7f
            + GradientNoise(x, y, 8, seed + 7) * 0.3f;
    }

    private static float GradientNoise(int x, int y, int cells, int seed)
    {
        var step = (float)NoiseSize / cells;
        var fx = x / step;
        var fy = y / step;
        var x0 = Mathf.FloorToInt(fx);
        var y0 = Mathf.FloorToInt(fy);
        var tx = fx - x0;
        var ty = fy - y0;

        float Corner(int cx, int cy, float dx, float dy)
        {
            var angle = Hash(((cx % cells) + cells) % cells, ((cy % cells) + cells) % cells, seed) * Mathf.PI * 2f;
            return Mathf.Cos(angle) * dx + Mathf.Sin(angle) * dy;
        }

        var u = Fade(tx);
        var v = Fade(ty);

        return Mathf.Lerp(
            Mathf.Lerp(Corner(x0, y0, tx, ty), Corner(x0 + 1, y0, tx - 1f, ty), u),
            Mathf.Lerp(Corner(x0, y0 + 1, tx, ty - 1f), Corner(x0 + 1, y0 + 1, tx - 1f, ty - 1f), u),
            v);
    }

    private static float Fade(float t) => t * t * t * (t * (t * 6f - 15f) + 10f);

    private static float Hash(int x, int y, int seed)
    {
        unchecked
        {
            var h = (uint)(x * 374761393 + y * 668265263 + seed * 2147483647);
            h = (h ^ (h >> 13)) * 1274126177u;
            h ^= h >> 16;
            return (h & 0xFFFFFF) / (float)0xFFFFFF;
        }
    }
}
