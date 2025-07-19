using Assets.Scripts.Infrastructure.Enums;
using Assets.Scripts.Infrastructure.Events;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitProducing : MonoBehaviour
{
    public GameObject CurrentProducingUnit = null;

    public List<GameObject> ProducingQueueInfo = new List<GameObject>();

    public float ProductionTime { get { return productionTime; } }
    public float CurrentProducingTimer { get { return currentProducingTimer; } }

    private TeamMember _teamMemeber;
    private UnitEventManager _unitEventManager;
    private UnitValues _unitValues;
    private PlayerResources _playerResources;

    private bool isProcessing = false;


    private Queue<GameObject> _producingQueue = new Queue<GameObject>();

    private float productionTime = 0f;
    private float currentProducingTimer = 0f;

    void Start()
    {
        _unitEventManager = GetComponent<UnitEventManager>();
        _teamMemeber = GetComponent<TeamMember>();
        _unitValues = GetComponent<UnitValues>();
        _playerResources = GameObject.FindGameObjectWithTag(Tag.PlayerController.ToString())
            .GetComponent<PlayerResources>();

        _unitEventManager.ProduceCommandReceived += ProduceCommandHandler;
        _playerResources.ResourceChanged += OnSupplyChanged;
    }

    public void ProduceCommandHandler(ProduceCommandReceivedEventArgs args)
    {
        var unitToProduce = _unitValues.UnitsToProduce.FirstOrDefault(u => u.GetComponent<UnitValues>().Id == args.UnitId);

        if (unitToProduce == null)
        {
            return;
        }

        var unitValues = unitToProduce.GetComponent<UnitValues>();
        var resourceCost = unitValues.ResourceCost.ToArray();

        if (!_playerResources.CheckIfCanSpendResources(resourceCost))
        {
            Debug.Log("Not enough resources!");
            return;
        }

        if (!_playerResources.CheckIfHaveSupply(resourceCost))
        {
            Debug.Log("Not enough supply!");
            return;
        }

        _playerResources.SpendResources(resourceCost);

        isProcessing = true;

        if (CurrentProducingUnit == null)
        {
            StartProducing(unitToProduce, unitValues);
        }
        else
        {
            ProducingQueueInfo.Add(unitToProduce);
            _producingQueue.Enqueue(unitToProduce);
        }
    }

    void Update()
    {
        if (isProcessing && CurrentProducingUnit != null)
        {
            currentProducingTimer -= Time.deltaTime;

            if (currentProducingTimer <= 0)
            {
                Bounds producerBounds = gameObject.GetComponent<Renderer>().bounds;

                Bounds unitBounds = CurrentProducingUnit.GetComponent<Renderer>().bounds;

                var center = producerBounds.center;

                var producerHalfSize = producerBounds.extents;

                var unitHalfSize = unitBounds.extents;

                var positionToSpawn = new Vector3(center.x + producerHalfSize.x + unitHalfSize.x, CurrentProducingUnit.transform.position.y, transform.position.z);

                var unit = Instantiate(CurrentProducingUnit, positionToSpawn, CurrentProducingUnit.transform.rotation);

                unit.GetComponent<TeamMember>().TeamId = _teamMemeber.TeamId;
                unit.GetComponent<UnitEventManager>().OnAMoveCommandReceived(positionToSpawn + new Vector3(Random.Range(1, 3), 0, Random.Range(-3, 3)));

                if (_producingQueue.Any())
                {
                    ProducingQueueInfo.RemoveAt(0);
                    var unitToProduce = _producingQueue.Dequeue();

                    var unitValues = unitToProduce.GetComponent<UnitValues>();
                    var resourceCost = unitValues.ResourceCost.ToArray();

                    StartProducing(unitToProduce, unitValues);

                    if (!_playerResources.CheckIfHaveSupply(resourceCost))
                    {
                        isProcessing = false;
                    }
                }

                else
                {
                    CurrentProducingUnit = null;
                    isProcessing = false;
                }
            }
        }
    }

    protected void OnSupplyChanged(ResourceChangedEventArgs args)
    {
        if (CurrentProducingUnit == null)
        {
            return;
        }

        if (args.Type != ResourceType.SupplyResource)
        {
            return;
        }

        var unitValues = CurrentProducingUnit.GetComponent<UnitValues>();
        var resourceCost = unitValues.ResourceCost.ToArray();
        isProcessing = _playerResources.CheckIfHaveSupply(resourceCost);
    }

    private void StartProducing(GameObject unit, UnitValues unitValues = null)
    {
        unitValues ??= unit.GetComponent<UnitValues>();

        CurrentProducingUnit = unit;
        productionTime = unitValues.ProducingTime;
        currentProducingTimer = productionTime;
    }
}
