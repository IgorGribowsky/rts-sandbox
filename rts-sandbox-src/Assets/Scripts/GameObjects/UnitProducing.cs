using Assets.Scripts;
using Assets.Scripts.Infrastructure.Enums;
using Assets.Scripts.Infrastructure.Events;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// The production queue of a building (M-011). One list: the first entry is
/// the unit in production, the rest wait. Before T-055 the waiting units were
/// kept twice, in a list and a queue synchronised by hand.
///
/// Resources are paid when a unit joins the queue and come back in full when
/// it is cancelled, even halfway through. Supply is not paid here at all: it
/// is taken when the unit is born.
/// </summary>
public class UnitProducing : MonoBehaviour
{
    /// <summary>
    /// Production started or stopped. The progress bar listens instead of
    /// asking every frame (T-029).
    /// </summary>
    public event System.Action<bool> ProducingStateChanged;

    /// <summary>A unit joined the queue, left it or was born (T-055).</summary>
    public event System.Action QueueChanged;

    /// <summary>The unit in production, null when the queue is empty.</summary>
    public UnitTypeData CurrentProducingUnit => _queue.Count > 0 ? _queue[0] : null;

    /// <summary>The whole queue, the unit in production first.</summary>
    public IReadOnlyList<UnitTypeData> Queue => _queue;

    public float ProductionTime { get { return productionTime; } }
    public float CurrentProducingTimer { get { return currentProducingTimer; } }

    /// <summary>0..1 — how far the unit in production has got.</summary>
    public float Progress => CurrentProducingUnit != null && productionTime > 0f
        ? Mathf.Clamp01(1f - currentProducingTimer / productionTime)
        : 0f;

    /// <summary>Something is in the queue but the clock stands: the supply limit is full.</summary>
    public bool IsStalled => CurrentProducingUnit != null && !isProcessing;

    /// <summary>Where the units born here are sent (T-030); null until the player sets it.</summary>
    public Vector3? RallyPoint => _rallyPoint;

    private TeamMember _teamMember;
    private UnitEventManager _unitEventManager;
    private BuildingValues _buildingValues;
    private PlayerResources _playerResources;
    private PlayerEventController _playerEventController;

    private bool isProcessing = false;

    private Vector3? _rallyPoint;
    private RallyFlag _rallyFlag;
    private Selectable _selectable;

    private readonly List<UnitTypeData> _queue = new List<UnitTypeData>();

    private float productionTime = 0f;
    private float currentProducingTimer = 0f;

    void Awake()
    {
        _unitEventManager = GetComponent<UnitEventManager>();
        _teamMember = GetComponent<TeamMember>();
        _buildingValues = GetComponent<BuildingValues>();
        _selectable = GetComponent<Selectable>();
        _playerResources = GameObject.FindGameObjectWithTag(Tag.PlayerController.ToString())
            .GetComponent<PlayerResources>();
        _playerEventController = GameObject.FindGameObjectWithTag(Tag.PlayerController.ToString())
            .GetComponent<PlayerEventController>();
    }

    private void OnEnable()
    {
        _unitEventManager.ProduceCommandReceived += ProduceCommandHandler;
        _unitEventManager.Canceled += OnCanceled;
        _playerEventController.ResourceChanged += OnSupplyChanged;
    }

    private void OnDisable()
    {
        _unitEventManager.ProduceCommandReceived -= ProduceCommandHandler;
        _unitEventManager.Canceled -= OnCanceled;
        _playerEventController.ResourceChanged -= OnSupplyChanged;
    }

    public void ProduceCommandHandler(ProduceCommandReceivedEventArgs args)
    {
        var unitToProduce = _buildingValues.UnitsToProduce.FirstOrDefault(u => u != null && u.Id == args.UnitId);

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

        _queue.Add(unitToProduce);

        if (_queue.Count == 1)
        {
            StartProducing(unitToProduce);
        }

        RaiseQueueChanged();
    }

    /// <summary>
    /// Takes the unit at this place out of the queue and gives its price back
    /// in full (M-011). The one in production loses its progress, and the next
    /// one starts. False when there is nothing at that place.
    /// </summary>
    public bool Cancel(int slot)
    {
        if (slot < 0 || slot >= _queue.Count)
        {
            return false;
        }

        var cancelled = _queue[slot];
        _queue.RemoveAt(slot);
        Refund(cancelled);

        if (slot == 0)
        {
            if (_queue.Count > 0)
            {
                StartProducing(_queue[0]);
            }
            else
            {
                StopProducing();
            }
        }

        RaiseQueueChanged();
        return true;
    }

    /// <summary>
    /// `Esc` on a finished building: the unit in production goes, its price
    /// comes back in full, the next one starts (M-011). While the building is
    /// still going up this component is off and Building takes the `Esc`.
    /// </summary>
    private void OnCanceled(CanceledEventArgs args)
    {
        Cancel(0);
    }

    /// <summary>
    /// Right click on the ground with this building selected (T-030). Only the
    /// units born from now on go here: whoever is already out keeps the order
    /// it got at birth.
    /// </summary>
    public void SetRallyPoint(Vector3 point)
    {
        point.y = 0.5f;
        _rallyPoint = point;

        if (_rallyFlag == null)
        {
            var team = GameServices.TeamController.Teams.FirstOrDefault(t => t.Id == _teamMember.TeamId);
            _rallyFlag = RallyFlag.Create(team != null ? team.Color : Color.white);
        }

        _rallyFlag.Place(point);
    }

    private void LateUpdate()
    {
        // Seen only while this very building is selected.
        if (_rallyFlag != null)
        {
            _rallyFlag.SetVisible(_selectable != null && _selectable.IsSelected);
        }
    }

    private void OnDestroy()
    {
        if (_rallyFlag != null)
        {
            Destroy(_rallyFlag.gameObject);
        }
    }

    void Update()
    {
        if (isProcessing && CurrentProducingUnit != null)
        {
            currentProducingTimer -= Time.deltaTime;

            if (currentProducingTimer <= 0)
            {
                var produced = CurrentProducingUnit;

                Bounds producerBounds = gameObject.GetComponent<Renderer>().bounds;

                var center = producerBounds.center;

                var producerHalfSize = producerBounds.extents;

                var unitHalfSize = UnitFactory.GetBodyExtents(produced);

                var body = produced.BodyPrefab.transform;

                var positionToSpawn = new Vector3(center.x + producerHalfSize.x + unitHalfSize.x, body.position.y, transform.position.z);

                _queue.RemoveAt(0);

                var unit = UnitFactory.Create(produced, positionToSpawn, body.rotation, _teamMember.TeamId);

                // The rally point as it stands right now: moving the flag later
                // does not call this unit back (T-030).
                if (_rallyPoint.HasValue)
                {
                    unit.GetComponent<UnitEventManager>().OnMoveCommandReceived(_rallyPoint.Value);
                }
                else
                {
                    unit.GetComponent<UnitEventManager>().OnAMoveCommandReceived(positionToSpawn + new Vector3(Random.Range(1, 3), 0, Random.Range(-3, 3)));
                }

                if (_queue.Count > 0)
                {
                    StartProducing(_queue[0]);
                }
                else
                {
                    StopProducing();
                }

                RaiseQueueChanged();
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
        var processing = _playerResources.CheckIfHaveSupply(resourceCost);
        if (processing != isProcessing)
        {
            isProcessing = processing;
            RaiseQueueChanged();
        }
    }

    private void StartProducing(UnitTypeData unit)
    {
        productionTime = unit.Stats.ProducingTime;
        currentProducingTimer = productionTime;

        // The next unit waits if the limit is full; the supply event starts it.
        isProcessing = _playerResources.CheckIfHaveSupply(unit.Stats.ResourceCost.ToArray());

        RaiseProducingStateChanged(true);
    }

    private void StopProducing()
    {
        isProcessing = false;
        productionTime = 0f;
        currentProducingTimer = 0f;
        RaiseProducingStateChanged(false);
    }

    /// <summary>Everything but supply goes back; supply was never paid (M-012).</summary>
    private void Refund(UnitTypeData unit)
    {
        foreach (var cost in unit.Stats.ResourceCost)
        {
            if (cost.Amount <= 0)
            {
                continue;
            }

            var resource = _playerResources.ResourcesAmount.FirstOrDefault(r => r.Resource == cost.Resource);
            if (resource == null || IsSupply(cost.Resource))
            {
                continue;
            }

            _playerResources.AddResource(cost.Resource, cost.Amount);
        }
    }

    private static bool IsSupply(ResourceDefinition resource)
    {
        return resource != null && resource.IsSupply;
    }

    private void RaiseProducingStateChanged(bool producing)
    {
        if (ProducingStateChanged != null)
        {
            ProducingStateChanged(producing);
        }
    }

    private void RaiseQueueChanged()
    {
        QueueChanged?.Invoke();
    }
}
