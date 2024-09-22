
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
        //if (_unitProducing.CurrentProducingTimer <= 0)
        //{
        //    return;
        //}

        var percent = _unitProducing.ProductionTime - _unitProducing.CurrentProducingTimer / _unitProducing.ProductionTime;

        UpdateBar(percent);
    }
}
