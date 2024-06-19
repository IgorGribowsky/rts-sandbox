using Assets.Scripts.Infrastructure.Enums;
using Assets.Scripts.Infrastructure.Events;
using UnityEngine;

public class HealthPointsBar : MonoBehaviour
{
    public GameObject Unit;
    public GameObject HpBar;

    private UnitValues _unitValues;
    private UnitEventManager _unitEventManager;

    // Start is called before the first frame update
    void Start()
    {
        if (Unit == null)
        {
            Unit = gameObject;
        }

        if (HpBar == null)
        {
            foreach (Transform transform in Unit.transform)
            {
                if (transform.CompareTag(Tag.HPBar.ToString()))
                {
                    HpBar = transform.gameObject;
                    break;
                }
            }
        }

        _unitEventManager = Unit.GetComponent<UnitEventManager>();
        _unitValues = Unit.GetComponent<UnitValues>();

        UpdateScale(new HealthPointsChangedEventArgs(_unitValues.CurrentHp));

        _unitEventManager.HealthPointsChanged += UpdateScale;
    }

    // Update is called once per frame
    protected void UpdateScale(HealthPointsChangedEventArgs args)
    {
        if (Unit == null || HpBar == null)
        {
            return;
        }

        var percent = args.CurrentHp / _unitValues.MaximumHp;
        if (percent < 1)
        {
            HpBar.SetActive(true);

            var currentHpBarScale = HpBar.transform.GetChild(1).localScale;

            var newCurrentHpBarScale = new Vector3(percent, currentHpBarScale.y, currentHpBarScale.z);

            HpBar.transform.GetChild(1).localScale = newCurrentHpBarScale;
        }
        else
        {
            HpBar.SetActive(false);
        }
    }
}
