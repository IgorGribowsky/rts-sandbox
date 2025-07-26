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
    private bool _miningIsProcessing = false;

    public void Awake()
    {
        _navmeshMovement = gameObject.GetComponent<NavMeshMovement>();
        _unitEventManager = GetComponent<UnitEventManager>();

        _unitEventManager.UnitDied += UnitDiedHandler;
        _navmeshMovement.NavMeshMovementArrive += HandleArrival;
    }

    public override void StartAction(EventArgs args)
    {
        EnableTriggerEndEvent();

        var actionArgs = args as MineActionStartedEventArgs;

        if (_mine != actionArgs.Mine)
        {
            _mine = actionArgs.Mine;
            _heldMineScript = _mine.GetComponent<HeldMine>();
            _miningIsProcessing = false;
            _navmeshMovement.GoToObject(_mine, GameConstants.MiningAcceptDistance);
        }
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
    }

    private void HandleArrival(EventArgs args)
    {
        if (!IsActive)
        {
            return;
        }

        var canAdd = _heldMineScript.ChechIfCanAddMiner();

        if (canAdd)
        {
            var pointToMine = _heldMineScript.GetMiningPoint();
            gameObject.transform.position = new Vector3(pointToMine.x, gameObject.transform.position.y, pointToMine.z);
            _heldMineScript.AddMiner(gameObject);
            _miningIsProcessing = true;
        }
        else
        {
            IsActive = false;
            if (TriggerEndEventFlag)
            {
                _unitEventManager.OnMineActionEnded();
            }

            Debug.Log("Unable to add miner!");
        }

        _navmeshMovement.Stop();
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

    private void OnDestroy()
    {
        _unitEventManager.UnitDied -= UnitDiedHandler;
        _navmeshMovement.NavMeshMovementArrive -= HandleArrival;
    }
}
