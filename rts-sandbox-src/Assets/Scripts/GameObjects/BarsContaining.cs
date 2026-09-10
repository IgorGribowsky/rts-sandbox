using Assets.Scripts.Infrastructure.Enums;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The rotation every bar in the scene needs, worked out once per frame.
///
/// A bar used to do <c>LookAt(transform.position + camera.forward)</c>, and the
/// result of that does not depend on where the unit stands at all — only on
/// where the camera looks. So one hundred units were computing one hundred
/// times the same quaternion, every frame (T-029).
/// </summary>
public static class BarsBillboard
{
    private static int _frame = -1;
    private static Quaternion _rotation = Quaternion.identity;

    public static Quaternion Rotation
    {
        get
        {
            if (_frame == Time.frameCount)
            {
                return _rotation;
            }

            _frame = Time.frameCount;

            var camera = Camera.main;
            if (camera != null)
            {
                _rotation = Quaternion.LookRotation(camera.transform.forward);
            }

            return _rotation;
        }
    }
}

/// <summary>
/// The bars hanging over one unit: keeps them facing the camera and stacked in
/// order of priority.
///
/// Does as little as it can get away with (T-029): nothing at all while the
/// unit is off screen — Unity tells us with OnBecameVisible — nothing while no
/// bar is showing, no turning unless the camera actually turned, and no
/// re-stacking unless a bar appeared or disappeared.
/// </summary>
public class BarsContaining : MonoBehaviour
{
    private readonly Dictionary<int, int> _barIdPriorityDict = new Dictionary<int, int>();
    private readonly List<GameObject> _barsList = new List<GameObject>();

    private Transform _barsContainer;

    /// <summary>Unity's own culling: is this unit's renderer on some camera.</summary>
    private bool _isVisible;

    /// <summary>At least one bar is switched on, so there is something to keep facing us.</summary>
    private bool _hasVisibleBar;

    private bool _needsReorder;
    private Quaternion _appliedRotation = Quaternion.identity;

    void Awake()
    {
        foreach (Transform child in gameObject.transform)
        {
            if (child.CompareTag(Tag.BarCanvas.ToString()))
            {
                _barsContainer = child;
            }
        }
    }

    /// <summary>
    /// Called by Unity when the unit's renderer enters a camera. In the editor
    /// the Scene view counts as a camera too, so bars stay awake there.
    /// </summary>
    private void OnBecameVisible()
    {
        _isVisible = true;
        ApplyContainerState();
    }

    private void OnBecameInvisible()
    {
        _isVisible = false;
        ApplyContainerState();
    }

    void Update()
    {
        if (!_isVisible || !_hasVisibleBar || _barsContainer == null)
        {
            return;
        }

        var rotation = BarsBillboard.Rotation;
        if (rotation != _appliedRotation)
        {
            _barsContainer.rotation = rotation;
            _appliedRotation = rotation;
        }

        if (_needsReorder)
        {
            ReOrderBars();
            _needsReorder = false;
        }
    }

    /// <summary>
    /// A bar was switched on or off. Called by <see cref="BarBase"/>: the stack
    /// only has to be rebuilt when its contents change, not every frame.
    /// </summary>
    public void OnBarVisibilityChanged()
    {
        _needsReorder = true;

        _hasVisibleBar = false;
        for (var i = 0; i < _barsList.Count; i++)
        {
            if (_barsList[i] != null && _barsList[i].activeSelf)
            {
                _hasVisibleBar = true;
                break;
            }
        }

        ApplyContainerState();
    }

    public void ReOrderBars()
    {
        var shift = 0f;
        for (var i = 0; i < _barsList.Count; i++)
        {
            var bar = _barsList[i];
            if (bar == null || !bar.activeSelf)
            {
                continue;
            }

            var rectTransform = (RectTransform)bar.transform;
            if (shift != 0f)
            {
                shift += rectTransform.rect.height / 2;
            }
            rectTransform.localPosition = new Vector3(rectTransform.localPosition.x, -shift, rectTransform.localPosition.z);
            shift += rectTransform.rect.height / 2;
        }
    }

    public GameObject AddBarToContainer(GameObject barTemplate, int priority)
    {
        var bar = Instantiate(barTemplate, _barsContainer);
        bar.transform.parent = _barsContainer;
        _barsList.Add(bar);
        _barIdPriorityDict.Add(bar.GetInstanceID(), priority);

        _barsList.Sort((a, b) => _barIdPriorityDict[b.GetInstanceID()].CompareTo(_barIdPriorityDict[a.GetInstanceID()]));

        OnBarVisibilityChanged();

        return bar;
    }

    /// <summary>
    /// The canvas itself is switched off while there is nothing to show or
    /// nobody to show it to: that takes the whole canvas out of the UI rebuild
    /// and out of rendering, not just this script out of Update.
    /// </summary>
    private void ApplyContainerState()
    {
        if (_barsContainer == null)
        {
            return;
        }

        var shouldBeOn = _isVisible && _hasVisibleBar;
        if (_barsContainer.gameObject.activeSelf != shouldBeOn)
        {
            _barsContainer.gameObject.SetActive(shouldBeOn);
        }

        if (shouldBeOn)
        {
            // Coming back into view: face the camera at once, do not wait a frame.
            _appliedRotation = BarsBillboard.Rotation;
            _barsContainer.rotation = _appliedRotation;
            ReOrderBars();
            _needsReorder = false;
        }
    }
}
