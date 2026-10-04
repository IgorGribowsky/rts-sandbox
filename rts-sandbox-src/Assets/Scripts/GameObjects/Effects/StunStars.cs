using Assets.Scripts.Infrastructure.Enums;
using Assets.Scripts.Infrastructure.Events;
using UnityEngine;

/// <summary>
/// Stars turning over the head of a stunned unit (M-019, T-069). One picture
/// lying flat on a quad; the turning is the material's own (_Spin of
/// RTS/GroundMark), so nothing ticks here but the position.
///
/// Follows the stun events, not the effect: StunEnded comes only when the last
/// stun is off, so two stuns in a row neither blink the stars nor drop them
/// early. Added by the first stun that lands, like UnitEffects.
/// </summary>
public class StunStars : MonoBehaviour
{
    private const float HeightAboveHead = 0.35f;
    private const float SizeToUnit = 1.3f;
    private const float MinimumSize = 0.9f;

    private UnitEventManager _events;
    private Renderer _unitRenderer;
    private GameObject _stars;

    public static StunStars GetOrAdd(GameObject unit)
    {
        if (unit == null)
        {
            return null;
        }

        var stars = unit.GetComponent<StunStars>();
        return stars != null ? stars : unit.AddComponent<StunStars>();
    }

    private void Awake()
    {
        _events = GetComponent<UnitEventManager>();
        _unitRenderer = GetComponent<Renderer>();
    }

    private void OnEnable()
    {
        if (_events != null)
        {
            _events.StunStarted += OnStunStarted;
            _events.StunEnded += OnStunEnded;
        }
    }

    private void OnDisable()
    {
        if (_events != null)
        {
            _events.StunStarted -= OnStunStarted;
            _events.StunEnded -= OnStunEnded;
        }

        if (_stars != null)
        {
            _stars.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (_stars != null)
        {
            Destroy(_stars);
        }
    }

    private void OnStunStarted(StunStartedEventArgs args)
    {
        if (_stars == null && !CreateStars())
        {
            return;
        }

        Follow();
        _stars.SetActive(true);
    }

    private void OnStunEnded(StunEndedEventArgs args)
    {
        if (_stars != null)
        {
            _stars.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        if (_stars != null && _stars.activeSelf)
        {
            Follow();
        }
    }

    /// <summary>
    /// Not a child of the unit: units are scaled cubes, and a child would come
    /// out stretched. It just keeps over the head.
    /// </summary>
    private void Follow()
    {
        var bounds = _unitRenderer != null ? _unitRenderer.bounds : new Bounds(transform.position, Vector3.one);
        _stars.transform.position = new Vector3(bounds.center.x, bounds.max.y + HeightAboveHead, bounds.center.z);
    }

    private bool CreateStars()
    {
        var material = GameObject.FindGameObjectWithTag(Tag.GameController.ToString())
            ?.GetComponent<GameController>()?.StunStarsMaterial;

        if (material == null)
        {
            Debug.LogError("GameController has no StunStarsMaterial: stunned units get no stars.", this);
            enabled = false;
            return false;
        }

        _stars = GameObject.CreatePrimitive(PrimitiveType.Quad);
        _stars.name = "StunStars (" + name + ")";
        Destroy(_stars.GetComponent<Collider>());

        var footprint = _unitRenderer != null
            ? Mathf.Max(_unitRenderer.bounds.size.x, _unitRenderer.bounds.size.z)
            : 1f;
        var size = Mathf.Max(MinimumSize, footprint * SizeToUnit);

        // A quad faces -Z; laid on its back it looks up at the camera.
        _stars.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        _stars.transform.localScale = new Vector3(size, size, 1f);

        var renderer = _stars.GetComponent<Renderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;

        return true;
    }
}
