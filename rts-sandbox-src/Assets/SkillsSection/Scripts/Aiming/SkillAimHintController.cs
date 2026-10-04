using Assets.Scripts.Infrastructure.Events;
using Assets.Scripts.Infrastructure.Helpers;
using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.SkillsSection.Scripts.Aiming
{
    /// <summary>
    /// Draws the aiming hints while a skill is aimed (M-020): the circle of the
    /// reach around the caster plus one hint picked by the action asset.
    ///
    /// The hints are pictures lying on the ground, in the manner of Dota and
    /// League of Legends (T-061): a range ring with ticks that turn slowly, a
    /// wide arrow with chevrons running along it, an area that breathes, a
    /// target mark that locks onto a unit in gold. How each one moves is set in
    /// its material (RTS/GroundMark); the colours are here.
    ///
    /// It only draws. Whether the cast is possible, how far it reaches and what it
    /// costs is decided by the skill system (M-015): SkillController turns this on
    /// exactly when a cast can be made and off when the key comes up, so a hint on
    /// screen always means "this cast is possible right now".
    /// </summary>
    public class SkillAimHintController : MonoBehaviour
    {
        [Header("Look")]
        public Material RangeMaterial;
        public Material AreaMaterial;
        public Material ArrowBodyMaterial;
        public Material ArrowHeadMaterial;
        public Material TargetMaterial;

        public Color RangeColor = new Color(1f, 1f, 1f, 0.35f);

        public Color HintColor = new Color(0.35f, 0.8f, 1f, 0.9f);

        [Tooltip("The target mark once it sits on a unit: the cast will go to this one.")]
        public Color LockedColor = new Color(1f, 0.8f, 0.32f, 0.95f);

        [Header("Shape")]
        [Tooltip("Height the hints are drawn at, to keep them off the ground surface.")]
        public float GroundHeight = 0.05f;

        public float ArrowWidth = 1.1f;

        public float ArrowHeadLength = 1.2f;

        [Tooltip("The arrow starts this far from the caster's centre.")]
        public float ArrowStartOffset = 0.6f;

        [Tooltip("Diameter of the target mark on the ground with no unit under it.")]
        public float TargetSize = 1.3f;

        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int TilingId = Shader.PropertyToID("_MainTex_ST");

        private PlayerEventController _playerEventController;

        private Transform _container;

        private Decal _range;
        private Decal _area;
        private Decal _arrowBody;
        private Decal _arrowHead;
        private Decal _target;

        private GameObject _caster;
        private ActiveSkill _skill;

        private Vector3 _cursorPosition;
        private GameObject _unitUnderCursor;

        /// <summary>Aiming has started: show the hints of this skill of this caster.</summary>
        public void Show(GameObject caster, ActiveSkill skill)
        {
            if (caster == null || skill == null)
            {
                Hide();
                return;
            }

            _caster = caster;
            _skill = skill;
            Redraw();
        }

        /// <summary>Aiming is over, for any reason at all.</summary>
        public void Hide()
        {
            _caster = null;
            _skill = null;
            HideAll();
        }

        void Awake()
        {
            _playerEventController = GetComponent<PlayerEventController>();
        }

        private void OnEnable()
        {
            if (_playerEventController != null)
            {
                _playerEventController.CursorMoved += CursorMovedHandler;
            }
        }

        private void OnDisable()
        {
            if (_playerEventController != null)
            {
                _playerEventController.CursorMoved -= CursorMovedHandler;
            }
        }

        void Start()
        {
            CreateDecals();
            HideAll();
        }

        void Update()
        {
            // The caster can die in the middle of aiming, and then there is nothing
            // to draw around any more.
            if (_caster == null || _skill == null)
            {
                if (_range != null && _range.Visible)
                {
                    Hide();
                }

                return;
            }

            Redraw();
        }

        private void Redraw()
        {
            if (_range == null)
            {
                return;
            }

            var casterPosition = OnGround(_caster.transform.position);
            var action = _skill.Action;
            var range = GetAimRange(action);

            if (range > 0f)
            {
                _range.Show(casterPosition, Vector3.forward, new Vector2(range * 2f, range * 2f), RangeColor);
            }
            else
            {
                _range.Hide();
            }

            HideHints();

            if (action == null)
            {
                return;
            }

            var aimPoint = ClampToRange(GetAimPoint(action), casterPosition, range);

            switch (action.AimHint)
            {
                case SkillAimHintType.Projectile:
                    DrawArrow(casterPosition, aimPoint);
                    break;
                case SkillAimHintType.Teleport:
                    _target.Show(aimPoint, Vector3.forward, new Vector2(TargetSize, TargetSize), HintColor);
                    break;
                case SkillAimHintType.TargetUnit:
                    DrawTarget(aimPoint);
                    break;
                case SkillAimHintType.Area:
                    var radius = GetAreaRadius(action);
                    if (radius > 0f)
                    {
                        _area.Show(aimPoint, Vector3.forward, new Vector2(radius * 2f, radius * 2f), HintColor);
                    }
                    break;
            }
        }

        /// <summary>
        /// How far the hint is allowed to reach: the range the action really has,
        /// and the CastRange of the skill only when the action sets no limit of its
        /// own (decision of the user, answer in chat 2026-09-07). Blink is why:
        /// its CastRange is 50 because the caster never walks for it, while the jump
        /// stops at 8.
        /// </summary>
        private float GetAimRange(ActiveSkillAction action)
        {
            var ownRange = action == null ? 0f : action.MaxRange;

            return ownRange > 0f ? ownRange : _skill.CastRange;
        }

        /// <summary>
        /// Where the hint points. Only the mark on a unit differs: it sticks to the
        /// centre of whoever is under the cursor, and falls back to the cursor when
        /// there is nobody. Allies and enemies look the same (M-020).
        /// </summary>
        private Vector3 GetAimPoint(ActiveSkillAction action)
        {
            if (action.AimHint == SkillAimHintType.TargetUnit && _unitUnderCursor != null)
            {
                return OnGround(_unitUnderCursor.transform.position);
            }

            return OnGround(_cursorPosition);
        }

        /// <summary>
        /// The hint never leaves the range circle: past it the point slides along the
        /// border while the direction keeps following the cursor (M-020).
        /// </summary>
        private static Vector3 ClampToRange(Vector3 point, Vector3 center, float range)
        {
            if (range <= 0f)
            {
                return center;
            }

            var offset = point - center;
            offset.y = 0f;

            if (offset.sqrMagnitude <= range * range)
            {
                return point;
            }

            return center + offset.normalized * range;
        }

        private static float GetAreaRadius(ActiveSkillAction action)
        {
            return action is CastToAreaAction areaAction ? areaAction.AreaRadius : 0f;
        }

        private Vector3 OnGround(Vector3 point)
        {
            return new Vector3(point.x, GroundHeight, point.z);
        }

        /// <summary>
        /// A wide ribbon from the caster to the aim point with chevrons running
        /// forward, and a head at the end. The chevrons keep their shape however
        /// long the arrow is: the picture repeats along it, not stretches.
        /// </summary>
        private void DrawArrow(Vector3 from, Vector3 to)
        {
            var direction = to - from;
            direction.y = 0f;

            // The cursor sits on the caster: there is no direction to point at yet.
            if (direction.sqrMagnitude < 0.0001f)
            {
                return;
            }

            var length = direction.magnitude;
            direction /= length;

            var head = Mathf.Min(ArrowHeadLength, length);
            var start = Mathf.Min(ArrowStartOffset, Mathf.Max(0f, length - head));
            var shaft = length - head - start;

            if (shaft > 0.01f)
            {
                var shaftCenter = from + direction * (start + shaft / 2f);
                _arrowBody.Show(shaftCenter, direction, new Vector2(ArrowWidth, shaft), HintColor,
                    new Vector4(1f, shaft / ArrowWidth, 0f, 0f));
            }

            var headCenter = to - direction * (head / 2f);
            _arrowHead.Show(headCenter, direction, new Vector2(ArrowWidth * 1.6f, head), HintColor);
        }

        /// <summary>
        /// The mark of a unit skill: on the spot under the cursor when there is
        /// nobody, round the unit's feet and gold when there is — the cast goes
        /// to this one (T-061).
        /// </summary>
        private void DrawTarget(Vector3 point)
        {
            if (_unitUnderCursor == null)
            {
                _target.Show(point, Vector3.forward, new Vector2(TargetSize, TargetSize), HintColor);
                return;
            }

            var size = Mathf.Max(TargetSize * 1.6f, _unitUnderCursor.GetSize() * 2.4f);
            _target.Show(point, Vector3.forward, new Vector2(size, size), LockedColor);
        }

        private void CreateDecals()
        {
            // At the root and not under the player controller: the hints live in world
            // coordinates and must not inherit anybody's transform.
            _container = new GameObject("SkillAimHints").transform;

            var quad = Resources.GetBuiltinResource<Mesh>("Quad.fbx");

            _range = new Decal("Range", _container, quad, RangeMaterial);
            _area = new Decal("Area", _container, quad, AreaMaterial);
            _arrowBody = new Decal("ArrowBody", _container, quad, ArrowBodyMaterial);
            _arrowHead = new Decal("ArrowHead", _container, quad, ArrowHeadMaterial);
            _target = new Decal("Target", _container, quad, TargetMaterial);
        }

        private void HideAll()
        {
            _range?.Hide();
            HideHints();
        }

        private void HideHints()
        {
            _area?.Hide();
            _arrowBody?.Hide();
            _arrowHead?.Hide();
            _target?.Hide();
        }

        private void CursorMovedHandler(CursorMovedEventArgs args)
        {
            _cursorPosition = args.CursorPosition;
            _unitUnderCursor = args.UnitUnderCursor;
        }

        private void OnDestroy()
        {
            if (_container != null)
            {
                Destroy(_container.gameObject);
            }
        }

        /// <summary>
        /// One picture lying flat on the ground: a quad turned face up, its V axis
        /// along the given direction. Colour and tiling go through a property
        /// block, so the materials stay shared and untouched.
        /// </summary>
        private sealed class Decal
        {
            private readonly Transform _transform;
            private readonly MeshRenderer _renderer;
            private readonly MaterialPropertyBlock _block = new MaterialPropertyBlock();

            public bool Visible => _renderer.enabled;

            public Decal(string name, Transform parent, Mesh quad, Material material)
            {
                var go = new GameObject(name);
                go.transform.SetParent(parent, false);
                go.AddComponent<MeshFilter>().sharedMesh = quad;

                _transform = go.transform;
                _renderer = go.AddComponent<MeshRenderer>();
                _renderer.sharedMaterial = material;
                _renderer.shadowCastingMode = ShadowCastingMode.Off;
                _renderer.receiveShadows = false;
                _renderer.lightProbeUsage = LightProbeUsage.Off;
                _renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
                _renderer.enabled = false;
            }

            public void Show(Vector3 center, Vector3 forward, Vector2 size, Color color, Vector4? tiling = null)
            {
                if (_renderer.sharedMaterial == null)
                {
                    return;
                }

                _transform.SetPositionAndRotation(center,
                    Quaternion.LookRotation(forward, Vector3.up) * Quaternion.Euler(90f, 0f, 0f));
                _transform.localScale = new Vector3(size.x, size.y, 1f);

                _block.SetColor(ColorId, color);
                _block.SetVector(TilingId, tiling ?? new Vector4(1f, 1f, 0f, 0f));
                _renderer.SetPropertyBlock(_block);
                _renderer.enabled = true;
            }

            public void Hide()
            {
                _renderer.enabled = false;
            }
        }
    }
}
