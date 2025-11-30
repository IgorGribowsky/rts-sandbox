
using UnityEngine;

public class ProducingBar : BarBase
{
    private UnitProducing _unitProducing;

    private bool locker = false;
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
            if (!locker)
            {
                UpdateBar(1);
                locker = true;
            }
            return;
        }
        else if (locker)
        {
            locker = false;
        }

        var percent = 1 - _unitProducing.CurrentProducingTimer / _unitProducing.ProductionTime;
        UpdateBar(percent);
    }
}
