using Assets.Scripts.Infrastructure.Events;
using System;
using UnityEngine;

/// <summary>
/// The one entry point for the player's orders and input modes (M-001, M-022).
/// The keyboard and the HUD buttons both call these methods and nothing else,
/// so a button and its hot key are the same action, not two look-alikes, and
/// the HUD can show the mode no matter what switched it on.
///
/// Modes: A-click, build menu, building placement. Skill aiming lives in
/// SkillController; this class repeats its changes so the HUD has one event
/// to listen to.
/// </summary>
public class CommandInput : MonoBehaviour
{
    private UnitsController _unitsController;
    private BuildingController _buildingController;
    private PlayerEventController _playerEventController;

    private bool _aClick;

    /// <summary>Tells whether the queue modifier is held; set by the keyboard input.</summary>
    public Func<bool> QueueModifierSource { get; set; }

    /// <summary>Any mode changed: A-click, build menu, placement, skill aiming.</summary>
    public event Action ModesChanged;

    public bool IsAClick => _aClick;

    public bool IsBuildMenuOpen => _buildingController.BuildingMenuMod;

    public bool IsPlacingBuilding => _buildingController.BuildingMod;

    /// <summary>Shift or whatever the input has for "add to the queue".</summary>
    public bool IsQueueModifierHeld => QueueModifierSource != null
        ? QueueModifierSource()
        : Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

    private void Awake()
    {
        _unitsController = GetComponent<UnitsController>();
        _buildingController = GetComponent<BuildingController>();
        _playerEventController = GetComponent<PlayerEventController>();
    }

    private void OnEnable()
    {
        _playerEventController.BuildingModChanged += OnModStateChanged;
        _playerEventController.BuildingMenuModChanged += OnModStateChanged;
    }

    private void OnDisable()
    {
        _playerEventController.BuildingModChanged -= OnModStateChanged;
        _playerEventController.BuildingMenuModChanged -= OnModStateChanged;
    }

    // --- A-click ------------------------------------------------------------

    /// <summary>`A` or the sword: the next click in the world attacks.</summary>
    public void EnterAClick()
    {
        // The menu closes: A-click and the build menu never stand together,
        // the keyboard could not get into both either.
        if (IsBuildMenuOpen)
        {
            _buildingController.DisableBuildingMenuMod();
        }

        SetAClick(true);
    }

    public void ExitAClick()
    {
        SetAClick(false);
    }

    // --- building -----------------------------------------------------------

    /// <summary>`B` from the normal mode, or the build button: open or close the menu.</summary>
    public void ToggleBuildMenu()
    {
        if (IsBuildMenuOpen)
        {
            _buildingController.DisableBuildingMenuMod();
            return;
        }

        ExitAClick();
        _buildingController.EnableBuildingMenuMod();
    }

    public void CloseBuildMenu()
    {
        _buildingController.DisableBuildingMenuMod();
    }

    /// <summary>A building letter in the menu, or a card: start placing it.</summary>
    public void ChooseBuilding(KeyCode key)
    {
        _buildingController.EnableBuildingMod(key);
    }

    public void CancelBuildingPlacement()
    {
        _buildingController.DisableBuildingMod();
    }

    // --- orders -------------------------------------------------------------

    public void Hold(bool addToQueue)
    {
        _unitsController.OnHoldKeyDown(addToQueue);
    }

    /// <summary>Digit 1..0 or a hire card: the N-th unit of the selected building.</summary>
    public void Produce(int index)
    {
        _unitsController.ProduceUnit(index);
    }

    /// <summary>`Esc` in the normal mode: cancel what the selection is building.</summary>
    public void Cancel()
    {
        _unitsController.OnCancelClick();
    }

    /// <summary>For SkillController: aiming started or stopped.</summary>
    public void NotifySkillAimingChanged()
    {
        ModesChanged?.Invoke();
    }

    private void SetAClick(bool state)
    {
        if (_aClick == state)
        {
            return;
        }

        _aClick = state;
        ModesChanged?.Invoke();
    }

    private void OnModStateChanged(ModStateChangedEventArgs args)
    {
        ModesChanged?.Invoke();
    }
}
