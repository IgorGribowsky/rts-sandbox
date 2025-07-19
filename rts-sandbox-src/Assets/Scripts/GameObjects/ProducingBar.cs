
using UnityEngine;

public class ProducingBar : BarBase
{
    private UnitProducing _unitProducing;

    // Start is called before the first frame update
    public void Start()
    {
        base.Start();

        _unitProducing = Unit.GetComponent<UnitProducing>();
    }

    void Update()
    {
        if (_unitProducing.ProductionTime == 0)
        {
            UpdateBar(1);
            return;
        }

        var percent = 1 - _unitProducing.CurrentProducingTimer / _unitProducing.ProductionTime;
        UpdateBar(percent);
    }
}
