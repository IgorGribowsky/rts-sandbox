using Assets.Scripts.Infrastructure.Enums;
using UnityEngine;

public abstract class BarBase : MonoBehaviour
{
    public GameObject Unit;
    public GameObject Bar;
    public GameObject BarTemplate;
    public int Priority;

    private Transform activeBar;

    private BarsContaining _barsContaining;

    public void Start()
    {
        if (Unit == null)
        {
            Unit = gameObject;
        }

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
            Bar.SetActive(true);

            var currentBarScale = activeBar.localScale;
            var newCurrentBarScale = new Vector3(percent, currentBarScale.y, currentBarScale.z);
            activeBar.localScale = newCurrentBarScale;
        }
        else if (Bar.activeSelf)
        {
            Bar.SetActive(false);
        }
    }
}
