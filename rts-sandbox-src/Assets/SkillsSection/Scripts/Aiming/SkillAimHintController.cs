using Assets.Scripts.Infrastructure.Events;
using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.SkillsSection.Scripts.Aiming
{
    /// <summary>
    /// Draws the aiming hints while a skill key is held (M-020): a thin circle of
    /// the reach around the caster plus one hint picked by the action asset.
    ///
    /// It only draws. Whether the cast is possible, how far it reaches and what it
    /// costs is decided by the skill system (M-015): SkillController turns this on
    /// exactly when a cast can be made and off when the key comes up, so a hint on
    /// screen always means "this cast is possible right now".
    /// </summary>
    public class SkillAimHintController : MonoBehaviour
    {
        [Header("Look")]
        [Tooltip("Material of every hint line. Empty means an unlit one is made at " +
                 "runtime, so the lines never turn magenta.")]
        public Material LineMaterial;

        public Color RangeColor = new Color(1f, 1f, 1f, 0.35f);

        public Color HintColor = new Color(0.35f, 0.8f, 1f, 0.9f);

        public float LineWidth = 0.08f;

        [Header("Shape")]
        [Tooltip("Height the hints are drawn at, to keep them off the ground surface.")]
        public float GroundHeight = 0.05f;

        [Range(12, 128)]
        public int CircleSegments = 64;

        [Tooltip("Half the length of a stroke of the cross.")]
        public float CrossSize = 0.5f;

        public float ArrowHeadLength = 0.8f;

        [Range(5f, 80f)]
        public float ArrowHeadAngle = 25f;

        private PlayerEventController _playerEventController;

        private Transform _container;

        private LineRenderer _rangeCircle;
        private LineRenderer _areaCircle;
        private LineRenderer _arrowShaft;
        private LineRenderer _arrowHead;
        private LineRenderer _crossFirst;
        private LineRenderer _crossSecond;

        /// <summary>Made only when nothing is assigned in the inspector.</summary>
        private Material _fallbackMaterial;

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

        void Start()
        {
            _playerEventController = GetComponent<PlayerEventController>();

            if (_playerEventController != null)
            {
                _playerEventController.CursorMoved += CursorMovedHandler;
            }

            CreateLines();
            HideAll();
        }

        void Update()
        {
            // The caster can die in the middle of aiming, and then there is nothing
            // to draw around any more.
            if (_caster == null || _skill == null)
            {
                if (_rangeCircle != null && _rangeCircle.enabled)
                {
                    Hide();
                }

                return;
            }

            Redraw();
        }

        private void Redraw()
        {
            if (_rangeCircle == null)
            {
                return;
            }

            var casterPosition = OnGround(_caster.transform.position);
            var action = _skill.Action;
            var range = GetAimRange(action);

            DrawCircle(_rangeCircle, casterPosition, range, RangeColor);

            HideHintLines();

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
                case SkillAimHintType.TargetUnit:
                    DrawCross(aimPoint);
                    break;
                case SkillAimHintType.Area:
                    DrawCircle(_areaCircle, aimPoint, GetAreaRadius(action), HintColor);
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
        /// Where the hint points. Only the cross on a unit differs: it sticks to the
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

        private void DrawCircle(LineRenderer line, Vector3 center, float radius, Color color)
        {
            if (line == null || radius <= 0f)
            {
                return;
            }

            line.loop = true;
            line.positionCount = CircleSegments;

            for (var i = 0; i < CircleSegments; i++)
            {
                var angle = i * 2f * Mathf.PI / CircleSegments;
                line.SetPosition(i, center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius));
            }

            Apply(line, color);
        }

        private void DrawArrow(Vector3 from, Vector3 to)
        {
            var direction = to - from;
            direction.y = 0f;

            // The cursor sits on the caster: there is no direction to point at yet.
            if (direction.sqrMagnitude < 0.0001f)
            {
                return;
            }

            direction.Normalize();

            _arrowShaft.loop = false;
            _arrowShaft.positionCount = 2;
            _arrowShaft.SetPosition(0, from);
            _arrowShaft.SetPosition(1, to);
            Apply(_arrowShaft, HintColor);

            var back = -direction * ArrowHeadLength;
            var left = Quaternion.AngleAxis(ArrowHeadAngle, Vector3.up) * back;
            var right = Quaternion.AngleAxis(-ArrowHeadAngle, Vector3.up) * back;

            _arrowHead.loop = false;
            _arrowHead.positionCount = 3;
            _arrowHead.SetPosition(0, to + left);
            _arrowHead.SetPosition(1, to);
            _arrowHead.SetPosition(2, to + right);
            Apply(_arrowHead, HintColor);
        }

        private void DrawCross(Vector3 center)
        {
            var first = new Vector3(CrossSize, 0f, CrossSize);
            var second = new Vector3(CrossSize, 0f, -CrossSize);

            _crossFirst.loop = false;
            _crossFirst.positionCount = 2;
            _crossFirst.SetPosition(0, center - first);
            _crossFirst.SetPosition(1, center + first);
            Apply(_crossFirst, HintColor);

            _crossSecond.loop = false;
            _crossSecond.positionCount = 2;
            _crossSecond.SetPosition(0, center - second);
            _crossSecond.SetPosition(1, center + second);
            Apply(_crossSecond, HintColor);
        }

        private void Apply(LineRenderer line, Color color)
        {
            line.startWidth = LineWidth;
            line.endWidth = LineWidth;
            line.startColor = color;
            line.endColor = color;
            line.enabled = true;
        }

        private void CreateLines()
        {
            // At the root and not under the player controller: the lines live in world
            // coordinates and must not inherit anybody's transform.
            _container = new GameObject("SkillAimHints").transform;

            _rangeCircle = CreateLine("RangeCircle");
            _areaCircle = CreateLine("AreaCircle");
            _arrowShaft = CreateLine("ArrowShaft");
            _arrowHead = CreateLine("ArrowHead");
            _crossFirst = CreateLine("CrossFirst");
            _crossSecond = CreateLine("CrossSecond");
        }

        private LineRenderer CreateLine(string lineName)
        {
            var lineObject = new GameObject(lineName);
            lineObject.transform.SetParent(_container, false);

            // Turned face up so the ribbon of the line lies flat on the ground
            // instead of standing towards the camera.
            lineObject.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

            var line = lineObject.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.alignment = LineAlignment.TransformZ;
            line.textureMode = LineTextureMode.Stretch;
            line.numCapVertices = 0;
            line.numCornerVertices = 2;
            line.shadowCastingMode = ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.lightProbeUsage = LightProbeUsage.Off;
            line.sharedMaterial = GetLineMaterial();
            line.enabled = false;

            return line;
        }

        private Material GetLineMaterial()
        {
            if (LineMaterial != null)
            {
                return LineMaterial;
            }

            if (_fallbackMaterial == null)
            {
                _fallbackMaterial = new Material(Shader.Find("Sprites/Default"));
            }

            return _fallbackMaterial;
        }

        private void HideAll()
        {
            if (_rangeCircle != null)
            {
                _rangeCircle.enabled = false;
            }

            HideHintLines();
        }

        private void HideHintLines()
        {
            if (_areaCircle == null)
            {
                return;
            }

            _areaCircle.enabled = false;
            _arrowShaft.enabled = false;
            _arrowHead.enabled = false;
            _crossFirst.enabled = false;
            _crossSecond.enabled = false;
        }

        private void CursorMovedHandler(CursorMovedEventArgs args)
        {
            _cursorPosition = args.CursorPosition;
            _unitUnderCursor = args.UnitUnderCursor;
        }

        private void OnDestroy()
        {
            if (_playerEventController != null)
            {
                _playerEventController.CursorMoved -= CursorMovedHandler;
            }

            if (_container != null)
            {
                Destroy(_container.gameObject);
            }

            if (_fallbackMaterial != null)
            {
                Destroy(_fallbackMaterial);
            }
        }
    }
}
