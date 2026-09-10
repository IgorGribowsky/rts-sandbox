using Assets.Scripts.Infrastructure.Enums;
using UnityEngine;

public abstract class BarBase : MonoBehaviour
{
    public GameObject Unit;
    public GameObject Bar;
    public GameObject BarTemplate;
    public int Priority;

    private Transform activeBar;

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

        foreach (Transform barChild in Bar.transform)
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

            var currentBarScale = activeBar.localScale;
            var newCurrentBarScale = new Vector3(percent, currentBarScale.y, currentBarScale.z);
            activeBar.localScale = newCurrentBarScale;
        }
        else if (Bar.activeSelf)
        {
            Bar.SetActive(false);
            _barsContaining.OnBarVisibilityChanged();
        }
    }
}
