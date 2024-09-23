using Assets.Scripts.Infrastructure.Events;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UnitProducing : MonoBehaviour
{
    public Queue<GameObject> ProducingQueue = new Queue<GameObject>();

    public float ProductionTime { get { return productionTime; } }
    public float CurrentProducingTimer { get { return currentProducingTimer; } }

    private TeamMember _teamMemeber;
    private UnitEventManager _unitEventManager;
    private UnitValues _unitValues;

    private bool isProcessing = false;

    private GameObject currentProducingUnit = null;

    private float productionTime = 0f;
    private float currentProducingTimer = 0f;

    void Start()
    {
        _unitEventManager = GetComponent<UnitEventManager>();
        _teamMemeber = GetComponent<TeamMember>();
        _unitValues = GetComponent<UnitValues>();

        _unitEventManager.ProduceCommandReceived += ProduceCommandHandler;
    }

    public void ProduceCommandHandler(ProduceCommandReceivedEventArgs args)
    {
        var unitToProduce = _unitValues.UnitsToProduce.FirstOrDefault(u => u.GetComponent<UnitValues>().Id == args.UnitId);

        if (unitToProduce == null)
        {
            return;
        }

        isProcessing = true;

        if (currentProducingUnit == null)
        {
            currentProducingUnit = unitToProduce;
            productionTime = currentProducingUnit.GetComponent<UnitValues>().ProducingTime;
            currentProducingTimer = productionTime;
        }
        else
        {
            ProducingQueue.Enqueue(unitToProduce);
        }
    }

    void Update()
    {
        if (isProcessing && currentProducingUnit != null)
        {
            currentProducingTimer -= Time.deltaTime;

            if (currentProducingTimer <= 0)
            {
                Bounds producerBounds = gameObject.GetComponent<Renderer>().bounds;

                Bounds unitBounds = currentProducingUnit.GetComponent<Renderer>().bounds;

                var center = producerBounds.center;

                var producerHalfSize = producerBounds.extents;

                var unitHalfSize = unitBounds.extents;

                var positionToSpawn = new Vector3(center.x + producerHalfSize.x + unitHalfSize.x, currentProducingUnit.transform.position.y, transform.position.z);

                var unit = Instantiate(currentProducingUnit, positionToSpawn, currentProducingUnit.transform.rotation);

                unit.GetComponent<TeamMember>().TeamId = _teamMemeber.TeamId;
                unit.GetComponent<UnitEventManager>().OnMoveCommandReceived(positionToSpawn + new Vector3(Random.Range(1, 3), 0, Random.Range(-3, 3)));

                if (ProducingQueue.Any())
                {
                    currentProducingUnit = ProducingQueue.Dequeue();
                    currentProducingTimer = currentProducingUnit.GetComponent<UnitValues>().ProducingTime;
                }
                else
                {
                    currentProducingUnit = null;
                    isProcessing = false;
                }
            }
        }
    }
}
