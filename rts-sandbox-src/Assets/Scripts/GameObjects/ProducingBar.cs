
using UnityEngine;

/// <summary>
/// The progress bar of a building's production queue.
///
/// Whether anything is being produced now comes as an event from
/// <see cref="UnitProducing"/>; while nothing is, the bar does nothing at all.
/// Before T-029 it asked the building about its state every frame of the game,
/// on every building that could produce.
/// </summary>
public class ProducingBar : BarBase
{
    private UnitProducing _unitProducing;

    private bool _producing;

    public new void Awake()
    {
        base.Awake();

        _unitProducing = Unit.GetComponent<UnitProducing>();
    }

    private void OnEnable()
    {
        if (_unitProducing != null)
        {
            _unitProducing.ProducingStateChanged += OnProducingStateChanged;
            _producing = _unitProducing.CurrentProducingUnit != null;
        }
    }

    private void OnDisable()
    {
        if (_unitProducing != null)
        {
            _unitProducing.ProducingStateChanged -= OnProducingStateChanged;
        }
    }

    public new void Start()
    {
        base.Start();

        // Nothing is in production at start, so the bar begins hidden.
        UpdateBar(1);
    }

    void Update()
    {
        if (!_producing || _unitProducing.ProductionTime == 0)
        {
            return;
        }

        var percent = 1 - _unitProducing.CurrentProducingTimer / _unitProducing.ProductionTime;
        UpdateBar(percent);
    }

    private void OnProducingStateChanged(bool producing)
    {
        _producing = producing;

        if (!producing)
        {
            UpdateBar(1);
        }
    }
}
