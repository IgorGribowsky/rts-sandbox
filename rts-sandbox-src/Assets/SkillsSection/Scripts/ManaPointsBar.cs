using Assets.SkillsSection.Scripts.Events;

public class ManaPointsBar : BarBase
{
    private ManaValues _manaValues;
    private UnitEventManager _unitEventManager;

    // Start is called before the first frame update
    public void Start()
    {
        base.Start();

        _unitEventManager = Unit.GetComponent<UnitEventManager>();
        _manaValues = Unit.GetComponent<ManaValues>();

        UpdateScale(new ManaPointsChangedEventArgs(_manaValues.CurrentMana));
        _unitEventManager.ManaPointsChanged += UpdateScale;
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

    private void OnDestroy()
    {
        // Start may never have run: an object destroyed in the frame it
        // appeared, or one never activated, reaches OnDestroy with this
        // still null.
        if (_unitEventManager == null)
        {
            return;
        }

        _unitEventManager.ManaPointsChanged -= UpdateScale;
    }
}
