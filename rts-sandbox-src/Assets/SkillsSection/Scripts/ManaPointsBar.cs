using Assets.SkillsSection.Scripts.Events;

public class ManaPointsBar : BarBase
{
    private ManaValues _manaValues;
    private UnitEventManager _unitEventManager;

    public new void Awake()
    {
        base.Awake();

        _unitEventManager = Unit.GetComponent<UnitEventManager>();
        _manaValues = Unit.GetComponent<ManaValues>();
    }

    private void OnEnable()
    {
        _unitEventManager.ManaPointsChanged += UpdateScale;
    }

    private void OnDisable()
    {
        _unitEventManager.ManaPointsChanged -= UpdateScale;
    }

    // Start is called before the first frame update
    public new void Start()
    {
        base.Start();

        UpdateScale(new ManaPointsChangedEventArgs(_manaValues.CurrentMana));
    }

    protected void UpdateScale(ManaPointsChangedEventArgs args)
    {
        if (_manaValues.MaximumMana == 0)
        {
            UpdateBar(0);
        }
        else
        {
            var percent = args.CurrentMana / _manaValues.MaximumMana;

            UpdateBar(percent);
        }
    }
}
