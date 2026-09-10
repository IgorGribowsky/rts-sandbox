using Assets.Scripts.Infrastructure.Events;

public class HealthPointsBar : BarBase
{
    private UnitValues _unitValues;
    private UnitEventManager _unitEventManager;

    public new void Awake()
    {
        base.Awake();

        _unitEventManager = Unit.GetComponent<UnitEventManager>();
        _unitValues = Unit.GetComponent<UnitValues>();
    }

    private void OnEnable()
    {
        _unitEventManager.HealthPointsChanged += UpdateScale;
    }

    private void OnDisable()
    {
        _unitEventManager.HealthPointsChanged -= UpdateScale;
    }

    // Start is called before the first frame update
    public new void Start()
    {
        base.Start();

        UpdateScale(new HealthPointsChangedEventArgs(_unitValues.CurrentHp));
    }

    protected void UpdateScale(HealthPointsChangedEventArgs args)
    {
        var percent = args.CurrentHp / _unitValues.MaximumHp;

        UpdateBar(percent);
    }
}
