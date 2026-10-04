using Assets.Scripts.Infrastructure.Events;
using System;
using UnityEngine;

/// <summary>What a hot key stands for, to show the press on its button.</summary>
public enum HotkeyAction
{
    AClick,
    Hold,
    Gather,
    BuildMenu,
    Produce,
    Skill,
}

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
    private SkillController _skillController;

    private bool _aClick;

    /// <summary>Tells whether the queue modifier is held; set by the keyboard input.</summary>
    public Func<bool> QueueModifierSource { get; set; }

    /// <summary>Any mode changed: A-click, build menu, placement, skill aiming.</summary>
    public event Action ModesChanged;

    /// <summary>
    /// A hot key was pressed. The HUD plays the press on the button or card
    /// of the same action, as if it had been clicked (M-022). The index is the
    /// card or skill number, -1 when it does not apply.
    /// </summary>
    public event Action<HotkeyAction, int> HotkeyPressed;

    public bool IsAClick => _aClick;

    public bool IsBuildMenuOpen => _buildingController.BuildingMenuMod;

    public bool IsPlacingBuilding => _buildingController.BuildingMod;

    /// <summary>Place of the aimed skill in the main unit's list, -1 when none.</summary>
    public int AimedSkillIndex => _skillController.AimedSkillIndex;

    public bool IsAimingSkillByClick => _skillController.IsAimingByClick;

    /// <summary>Shift or whatever the input has for "add to the queue".</summary>
    public bool IsQueueModifierHeld => QueueModifierSource != null
        ? QueueModifierSource()
        : Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

    private void Awake()
    {
        _unitsController = GetComponent<UnitsController>();
        _buildingController = GetComponent<BuildingController>();
        _playerEventController = GetComponent<PlayerEventController>();
        _skillController = GetComponent<SkillController>();
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

        _skillController.CancelAiming();
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
        _skillController.CancelAiming();
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

    // --- skills -------------------------------------------------------------

    /// <summary>
    /// A skill button: aim the skill at this place, as a pressed key would.
    /// Other modes give way, the same as they never stand together with aiming
    /// on the keyboard. False when the skill cannot be cast right now.
    /// </summary>
    public bool AimSkill(int index)
    {
        if (!_skillController.PrepareSkillCastByClick(index))
        {
            return false;
        }

        SetAClick(false);

        if (IsPlacingBuilding)
        {
            _buildingController.DisableBuildingMod();
        }
        else if (IsBuildMenuOpen)
        {
            _buildingController.DisableBuildingMenuMod();
        }

        return true;
    }

    /// <summary>Left click in the world with a button-aimed skill: cast it there.</summary>
    public void CastAimedSkill(bool addToQueue)
    {
        _skillController.CommandAimedSkillCast(addToQueue);
    }

    public void CancelSkillAiming()
    {
        _skillController.CancelAiming();
    }

    // --- orders -------------------------------------------------------------

    public void Hold(bool addToQueue)
    {
        _unitsController.OnHoldKeyDown(addToQueue);
    }

    /// <summary>`+` or the gather button: workers go to the nearest work (M-023).</summary>
    public void Gather(bool addToQueue)
    {
        _unitsController.OnGatherKeyDown(addToQueue);
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

    public void NotifyHotkey(HotkeyAction action, int index = -1)
    {
        HotkeyPressed?.Invoke(action, index);
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
