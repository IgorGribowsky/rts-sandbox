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
/// its SightRange. The grid goes into a small texture: the camera darkens the
/// world by it (<see cref="FogOfWarEffect"/>), the minimap lays it over the map.
/// Between two passes the picture slides from the old texture to the new, so
/// the edge does not jump.
///
/// Switched on and off by GameController.FogOfWarEnabled, also in play mode.
/// Off, nothing is counted and nothing is drawn; what was explored is kept.
/// </summary>
public class FogOfWar : MonoBehaviour
{
    // Darkness written into the texture's alpha: black, grey, clear. The
    // shader and the minimap read only the alpha.
    private const byte UnexploredDarkness = 255;
    private const byte ExploredDarkness = 128;

    [Header("Grid")]
    [Tooltip("Side of one fog cell, metres. Smaller is a rounder edge and a dearer pass. Read at start.")]
    [Min(0.25f)]
    public float CellSize = 1f;

    [Tooltip("How far the fog reaches past the map rectangle, metres: units can walk a little beyond it. Read at start.")]
    [Min(0f)]
    public float EdgeMargin = 10f;

    [Tooltip("Seconds between two passes of sight.")]
    [Min(0.02f)]
    public float UpdateInterval = 0.15f;

    [Header("Look")]
    public Shader FogShader;

    [Tooltip("Where nobody has looked yet.")]
    public Color UnexploredColor = Color.black;

    [Tooltip("How much darker the explored ground is.")]
    [Range(0f, 1f)]
    public float ExploredDarkening = 0.5f;

    [Tooltip("How much colour the explored ground loses.")]
    [Range(0f, 1f)]
    public float ExploredDesaturation = 0.75f;

    [Tooltip("Width of the soft edge, in cells.")]
    [Range(0f, 4f)]
    public float EdgeSoftness = 1.5f;

    [Tooltip("Slow drifting haze over the explored ground. Zero turns it off.")]
    [Range(0f, 0.5f)]
    public float HazeStrength = 0.12f;

    [Tooltip("Size of the haze patches, metres.")]
    [Min(1f)]
    public float HazeScale = 14f;

    [Tooltip("How fast the haze drifts, metres per second.")]
    public float HazeSpeed = 0.6f;

    [Header("Measured")]
    [Tooltip("What the last pass of sight cost, milliseconds. Read only.")]
    public float LastPassMilliseconds;

    private GameController _game;
    private FogGrid _grid;
    private Rect _area;
    private byte[] _pixels;
    private Texture2D _current;
    private Texture2D _previous;
    private float _passTime;
    private float _timer;
    private bool _wasOn;
    private readonly HashSet<int> _sharedTeams = new HashSet<int>();
    private FogOfWarEffect _effect;
    private readonly System.Diagnostics.Stopwatch _stopwatch = new System.Diagnostics.Stopwatch();

    /// <summary>The fog is on: counted and drawn.</summary>
    public bool IsOn => _game != null && _game.FogOfWarEnabled;

    /// <summary>
    /// The newest pass: white, alpha is the darkness. Covers <see cref="Area"/>,
    /// bottom row is the smallest Z.
    /// </summary>
    public Texture2D Texture => _current;

    /// <summary>The previous pass, to slide from.</summary>
    public Texture2D PreviousTexture => _previous;

    /// <summary>How far the picture has slid from the previous pass to the newest, 0..1.</summary>
    public float Blend => Mathf.Clamp01((Time.time - _passTime) / UpdateInterval);

    /// <summary>The ground the fog covers, x and z in metres: the map plus the margin.</summary>
    public Rect Area => _area;

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
        _pixels = new byte[width * height * 4];
        _current = CreateTexture("Fog Of War");
        _previous = CreateTexture("Fog Of War Previous");
        Upload(_current);
        Upload(_previous);
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
            _wasOn = false;
            return;
        }

        _timer -= Time.deltaTime;
        if (_wasOn && _timer > 0f)
        {
            return;
        }

        _timer = UpdateInterval;
        Pass(snap: !_wasOn);
        _wasOn = true;
    }

    private void OnDestroy()
    {
        if (GameServices.FogOfWar == this)
        {
            GameServices.FogOfWar = null;
        }

        if (_effect != null)
        {
            Destroy(_effect);
        }

        Destroy(_current);
        Destroy(_previous);
    }

    /// <summary>
    /// One pass of sight. Right after the fog is switched on there is
    /// nothing worth sliding from, so both textures get the new picture.
    /// </summary>
    private void Pass(bool snap)
    {
        _stopwatch.Restart();

        _grid.BeginPass();

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
                sight / CellSize);
        }

        var cells = _grid.Cells;
        for (var i = 0; i < cells.Length; i++)
        {
            _pixels[i * 4 + 3] = cells[i] == FogState.Visible ? (byte)0
                : cells[i] == FogState.Explored ? ExploredDarkness
                : UnexploredDarkness;
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

    private Texture2D CreateTexture(string name)
    {
        // White with darkness in alpha: the minimap tints it, the shader reads alpha.
        for (var i = 0; i < _pixels.Length; i++)
        {
            _pixels[i] = (i & 3) == 3 ? UnexploredDarkness : (byte)255;
        }

        return new Texture2D(_grid.Width, _grid.Height, TextureFormat.RGBA32, false)
        {
            name = name,
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.HideAndDontSave,
        };
    }

    private void Upload(Texture2D texture)
    {
        texture.LoadRawTextureData(_pixels);
        texture.Apply(false, false);
    }
}
