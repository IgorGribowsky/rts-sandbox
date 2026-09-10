using Assets.Scripts.Infrastructure.Enums;
using Assets.Scripts.Infrastructure.Events;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitProducing : MonoBehaviour
{
    public UnitTypeData CurrentProducingUnit = null;

    public List<UnitTypeData> ProducingQueueInfo = new List<UnitTypeData>();

    public float ProductionTime { get { return productionTime; } }
    public float CurrentProducingTimer { get { return currentProducingTimer; } }

    private TeamMember _teamMemeber;
    private UnitEventManager _unitEventManager;
    private UnitValues _unitValues;
    private PlayerResources _playerResources;
    private PlayerEventController _playerEventController;

    private bool isProcessing = false;


    private Queue<UnitTypeData> _producingQueue = new Queue<UnitTypeData>();

    private float productionTime = 0f;
    private float currentProducingTimer = 0f;

    void Start()
    {
        _unitEventManager = GetComponent<UnitEventManager>();
        _teamMemeber = GetComponent<TeamMember>();
        _unitValues = GetComponent<UnitValues>();
        _playerResources = GameObject.FindGameObjectWithTag(Tag.PlayerController.ToString())
            .GetComponent<PlayerResources>();
        _playerEventController = GameObject.FindGameObjectWithTag(Tag.PlayerController.ToString())
            .GetComponent<PlayerEventController>();

        _unitEventManager.ProduceCommandReceived += ProduceCommandHandler;
        _playerEventController.ResourceChanged += OnSupplyChanged;
    }

    public void ProduceCommandHandler(ProduceCommandReceivedEventArgs args)
    {
        var unitToProduce = _unitValues.UnitsToProduce.FirstOrDefault(u => u != null && u.Id == args.UnitId);

        if (unitToProduce == null)
        {
            return;
        }

        var resourceCost = unitToProduce.Stats.ResourceCost.ToArray();

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
            StartProducing(unitToProduce);
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

                var center = producerBounds.center;

                var producerHalfSize = producerBounds.extents;

                var unitHalfSize = UnitFactory.GetBodyExtents(CurrentProducingUnit);

                var body = CurrentProducingUnit.BodyPrefab.transform;

                var positionToSpawn = new Vector3(center.x + producerHalfSize.x + unitHalfSize.x, body.position.y, transform.position.z);

                var unit = UnitFactory.Create(CurrentProducingUnit, positionToSpawn, body.rotation, _teamMemeber.TeamId);

                unit.GetComponent<UnitEventManager>().OnAMoveCommandReceived(positionToSpawn + new Vector3(Random.Range(1, 3), 0, Random.Range(-3, 3)));

                if (_producingQueue.Any())
                {
                    ProducingQueueInfo.RemoveAt(0);
                    var unitToProduce = _producingQueue.Dequeue();

                    var resourceCost = unitToProduce.Stats.ResourceCost.ToArray();

                    StartProducing(unitToProduce);

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

        var resourceCost = CurrentProducingUnit.Stats.ResourceCost.ToArray();
        isProcessing = _playerResources.CheckIfHaveSupply(resourceCost);
    }

    private void StartProducing(UnitTypeData unit)
    {
        CurrentProducingUnit = unit;
        productionTime = unit.Stats.ProducingTime;
        currentProducingTimer = productionTime;
    }

    private void OnDestroy()
    {
        // OnDestroy runs even when Start never did — an object destroyed in the
        // same frame it was created, or one that was never activated. Then these
        // fields are still null. Checked with != null and not with ?., because
        // ?. does not know about Unity's fake-null for destroyed objects.
        if (_unitEventManager != null)
        {
            _unitEventManager.ProduceCommandReceived -= ProduceCommandHandler;
        }

        if (_playerEventController != null)
        {
            _playerEventController.ResourceChanged -= OnSupplyChanged;
        }
    }
}
