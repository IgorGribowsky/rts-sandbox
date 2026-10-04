using Assets.Scripts.Infrastructure.Enums;
using UnityEngine;

/// <summary>
/// One bar over a unit (M-017): health, mana, production. The bar template
/// has a child tagged ActiveBar, the fill, anywhere inside it.
///
/// The fill is shortened by its right anchor, not squeezed by its scale
/// (T-059): the fill is a 9-sliced picture with round ends, and a squeezed
/// picture would squash its ends too.
/// </summary>
public abstract class BarBase : MonoBehaviour
{
    public GameObject Unit;
    public GameObject Bar;
    public GameObject BarTemplate;
    public int Priority;

    private RectTransform activeBar;

    protected BarsContaining _barsContaining;

    /// <summary>
    /// Who the bar belongs to. Resolved in Awake, because a subclass subscribes
    /// to that unit in OnEnable, and OnEnable runs before Start.
    /// </summary>
    public void Awake()
    {
        if (Unit == null)
        {
            Unit = gameObject;
        }
    }

    public void Start()
    {
        _barsContaining = Unit.GetComponent<BarsContaining>();

        if (Bar == null)
        {
            Bar = _barsContaining.AddBarToContainer(BarTemplate, Priority);
        }

        foreach (var barChild in Bar.GetComponentsInChildren<RectTransform>(true))
        {
            if (barChild.CompareTag(Tag.ActiveBar.ToString()))
            {
                activeBar = barChild;
                break;
            }
        }
    }

    protected void UpdateBar(float percent)
    {
        if (Bar == null)
        {
            return;
        }

        if (percent < 1)
        {
            // SetActive used to be called every frame even when nothing changed;
            // the container only needs telling when the stack actually changes
            // (T-029).
            if (!Bar.activeSelf)
            {
                Bar.SetActive(true);
                _barsContaining.OnBarVisibilityChanged();
            }

            var anchorMax = activeBar.anchorMax;
            var fill = Mathf.Clamp01(percent);
            if (!Mathf.Approximately(anchorMax.x, fill))
            {
                activeBar.anchorMax = new Vector2(fill, anchorMax.y);
            }
        }
        else if (Bar.activeSelf)
        {
            Bar.SetActive(false);
            _barsContaining.OnBarVisibilityChanged();
        }
    }
}
