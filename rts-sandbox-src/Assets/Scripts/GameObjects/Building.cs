using Assets.Scripts.Infrastructure.Constants;
using UnityEngine;

public class Building : MonoBehaviour
{
    public bool BuildingIsInProgress { get; set; }

    private UnitValues _unitValues;
    private UnitEventManager _unitEventManager;
    
    private float timeToBuild;
    private float hpToBuild;
    private float hpDelta;
    private float currentTimer;

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

        BuildingIsInProgress = true;
    }

    public void Update()
    {
        if (BuildingIsInProgress)
        {
            if (currentTimer < timeToBuild)
            {
                currentTimer += Time.deltaTime;
                _unitValues.CurrentHp += hpDelta * Time.deltaTime;
            }
            else
            {
                _unitValues.CurrentHp = Mathf.Round(_unitValues.CurrentHp);
                BuildingIsInProgress = false;
            }

            _unitEventManager.OnHealthPointsChanged(_unitValues.CurrentHp);
        }
    }
}