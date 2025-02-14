using Assets.Scripts.GameObjects.UnitBehaviour;
using Assets.Scripts.Infrastructure.Constants;
using Assets.Scripts.Infrastructure.Events;
using Assets.Scripts.Infrastructure.Helpers;
using System;
using UnityEngine;

public class MiningBehaviour : UnitBehaviourBase
{
    private NavMeshMovement _navmeshMovement;
    private UnitEventManager _unitEventManager;

    private HeldMine _heldMineScript;

    private GameObject _mine = null;
    private Vector3 _point;
    private bool _miningIsProcessing = false;

    public void Awake()
    {
        _navmeshMovement = gameObject.GetComponent<NavMeshMovement>();
        _unitEventManager = GetComponent<UnitEventManager>();
    }

    public override void StartAction(EventArgs args)
    {
        EnableTriggerEndEvent();

        _unitEventManager.UnitDied += UnitDiedHandler;

        var actionArgs = args as MineActionStartedEventArgs;

        _mine = actionArgs.Mine;
        _heldMineScript = _mine.GetComponent<HeldMine>();
        _point = gameObject.GetClosestPointToInteract(_mine.transform.position, _mine.GetComponent<BuildingValues>().ObstacleSize);
        _miningIsProcessing = false;
    }

    protected override void PreUpdate()
    {
        if (IsActive == false && _mine != null)
        {
            _heldMineScript.RemoveMiner(gameObject);
            _mine = null;
            _miningIsProcessing = false;
        }
    }

    protected override void UpdateAction()
    {
        if (_mine == null)
        {
            IsActive = false;
            _miningIsProcessing = false;
            _navmeshMovement.Stop();
            if (TriggerEndEventFlag)
            {
                _unitEventManager.OnMineActionEnded();
            }
            return;
        }

        if (!_miningIsProcessing)
        {
            var distanceToTarget = gameObject.GetDistanceTo(_mine);

            if (distanceToTarget <= GameConstants.MiningAcceptDistance)
            {

                var canAdd = _heldMineScript.ChechIfCanAddMiner();

                if (canAdd)
                {
                    var pointToMine = _heldMineScript.GetMiningPoint();
                    gameObject.transform.position = new Vector3(pointToMine.x, gameObject.transform.position.y, pointToMine.z);
                    _heldMineScript.AddMiner(gameObject);
                    _navmeshMovement.Stop();
                    _miningIsProcessing = true;
                }
                else
                {
                    IsActive = false;
                    _navmeshMovement.Stop();
                    if (TriggerEndEventFlag)
                    {
                        _unitEventManager.OnMineActionEnded();
                    }

                    Debug.Log("The mine is full!");
                }
            }
            else
            {
                _navmeshMovement.Go(_point);
            }
        }
    }

    protected void UnitDiedHandler(DiedEventArgs args)
    {
        if (_mine != null)
        {
            _heldMineScript.RemoveMiner(gameObject);
            _mine = null;
            _miningIsProcessing = false;
        }
    }
}
