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

    public override UnitActionType Trigger => UnitActionType.Mine;

    protected override void OnInitialize()
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

        // On the way to the mine and in it, workers walk through each other (T-063).
        _navmeshMovement.SetPassThrough(true);

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
            _navmeshMovement.SetPassThrough(false);
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

        var canAdd = _heldMineScript.CheckIfCanAddMiner();

        if (canAdd)
        {
            var pointToMine = _heldMineScript.GetMiningPoint();
            transform.position = new Vector3(pointToMine.x, transform.position.y, pointToMine.z);
            _heldMineScript.AddMiner(gameObject);
            _miningIsProcessing = true;
            _navmeshMovement.Stop();
            return;
        }

        Debug.Log("Unable to add miner!");

        // Everything is cleaned up BEFORE the end event: the event starts the
        // next queued command right away, and a Stop() after it used to cancel
        // that command's walk on the spot (T-035). The mine is forgotten too,
        // or an order to the same mine would be taken for "already going".
        IsActive = false;
        _mine = null;
        _heldMineScript = null;
        _miningIsProcessing = false;
        _navmeshMovement.SetPassThrough(false);
        _navmeshMovement.Stop();

        if (TriggerEndEventFlag)
        {
            _unitEventManager.OnMineActionEnded();
        }
    }

    protected override void OnDeactivated()
    {
        _navmeshMovement.SetPassThrough(false);
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

    public override void Dispose()
    {
        _unitEventManager.UnitDied -= UnitDiedHandler;
        _navmeshMovement.NavMeshMovementArrive -= HandleArrival;
    }
}
