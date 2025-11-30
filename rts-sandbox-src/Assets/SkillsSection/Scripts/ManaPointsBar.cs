using Assets.SkillsSection.Scripts.Events;

public class ManaPointsBar : BarBase
{
    private UnitValues _unitValues;
    private UnitEventManager _unitEventManager;

    // Start is called before the first frame update
    public void Start()
    {
        base.Start();

        _unitEventManager = Unit.GetComponent<UnitEventManager>();
        _unitValues = Unit.GetComponent<UnitValues>();

        UpdateScale(new ManaPointsChangedEventArgs(_unitValues.CurrentMana));
        _unitEventManager.ManaPointsChanged += UpdateScale;
    }

    protected void UpdateScale(ManaPointsChangedEventArgs args)
    {
        if (_unitValues.MaximumMana == 0)
        {
            UpdateBar(0);
        }
        else
        {
            var percent = args.CurrentMana / _unitValues.MaximumMana;

            UpdateBar(percent);
        }
    }

    private void OnDestroy()
    {
        _unitEventManager.ManaPointsChanged -= UpdateScale;
    }
}
