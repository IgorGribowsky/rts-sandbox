using Assets.Scripts.Infrastructure.Events;

public class HealthPointsBar : BarBase
{
    private UnitValues _unitValues;
    private UnitEventManager _unitEventManager;

    // Start is called before the first frame update
    public void Start()
    {
        base.Start();

        _unitEventManager = Unit.GetComponent<UnitEventManager>();
        _unitValues = Unit.GetComponent<UnitValues>();

        UpdateScale(new HealthPointsChangedEventArgs(_unitValues.CurrentHp));
        _unitEventManager.HealthPointsChanged += UpdateScale;
    }

    protected void UpdateScale(HealthPointsChangedEventArgs args)
    {
        var percent = args.CurrentHp / _unitValues.MaximumHp;

        UpdateBar(percent);
    }
}
