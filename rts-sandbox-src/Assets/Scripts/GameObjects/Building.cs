using Assets.Scripts.Infrastructure.Constants;
using UnityEngine;

public class Building : MonoBehaviour
{
    private UnitValues _unitValues;
    private UnitEventManager _unitEventManager;
    
    private float timeToBuild;
    private float hpToBuild;
    private float hpDelta;
    private float currentTimer;
    private bool buildingIsInProgress;

    public void Awake()
    {
        _unitValues = GetComponent<UnitValues>();
        _unitEventManager = GetComponent<UnitEventManager>();
    }

    public void Start()
    {

    }

    public void Build()
    {
        timeToBuild = _unitValues.ProducingTime;

        _unitValues.CurrentHp = _unitValues.MaximumHp * GameConstants.BuildingHPStartPercent;

        hpToBuild = _unitValues.MaximumHp - _unitValues.CurrentHp;

        hpDelta = hpToBuild / timeToBuild;

        currentTimer = 0f;

        buildingIsInProgress = true;
    }

    public void Update()
    {
        if (buildingIsInProgress)
        {
            if (currentTimer < timeToBuild)
            {
                currentTimer += Time.deltaTime;
                _unitValues.CurrentHp += hpDelta * Time.deltaTime;
            }
            else
            {
                _unitValues.CurrentHp = Mathf.Round(_unitValues.CurrentHp);
                buildingIsInProgress = false;
            }

            _unitEventManager.OnHealthPointsChanged(_unitValues.CurrentHp);
        }
    }
}