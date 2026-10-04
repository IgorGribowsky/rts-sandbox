using Assets.Scripts.Infrastructure.Events;
using Assets.SkillsSection.Scripts;
using Assets.SkillsSection.Scripts.Aiming;
using System.Linq;
using UnityEngine;
using static Assets.SkillsSection.Scripts.UnitSkills;

/// <summary>
/// Aiming and casting a skill with the current selection. Lives apart from
/// UnitsController on purpose: selecting units and giving orders should not
/// know anything about how a skill is aimed.
///
/// Skills are addressed by their place in the unit's list, not by key: a
/// button of the HUD knows the place, a key is turned into it. Two ways to
/// aim lead to one cast (M-023):
///   key  — pressed: aiming; released: the cast goes out at the cursor;
///   button — clicked: aiming; a left click in the world: the cast goes out.
/// </summary>
public class SkillController : MonoBehaviour
{
    private UnitsController _unitsController;
    private CommandInput _commands;
    private PlayerEventController _events;

    private int _playerTeamId;

    private UnitSkill _preparedSkill = null;
    private int _preparedIndex = -1;
    private bool _preparedByClick;

    /// <summary>Optional: without it aiming simply shows nothing (M-020).</summary>
    private SkillAimHintController _aimHints;

    /// <summary>Place of the skill being aimed in the unit's list, -1 when none.</summary>
    public int AimedSkillIndex => _preparedSkill != null ? _preparedIndex : -1;

    /// <summary>Aimed from a HUD button: the cast waits for a left click in the world.</summary>
    public bool IsAimingByClick => _preparedSkill != null && _preparedByClick;

    void Awake()
    {
        _events = GetComponent<PlayerEventController>();
    }

    void Start()
    {
        _unitsController = GetComponent<UnitsController>();
        _commands = GetComponent<CommandInput>();
        _playerTeamId = GetComponent<PlayerTeamMember>().TeamId;
        _aimHints = GetComponent<SkillAimHintController>();
    }

    private void OnEnable()
    {
        _events.SelectionChanged += OnSelectionChanged;
    }

    private void OnDisable()
    {
        _events.SelectionChanged -= OnSelectionChanged;
    }

    // --- keys -------------------------------------------------------------------

    /// <summary>Key pressed: remember which skill of which caster is being aimed.</summary>
    public bool PrepareSkillCast(KeyCode keyCode)
    {
        var index = IndexOfKey(keyCode);
        return index >= 0 && Prepare(index, false);
    }

    /// <summary>Key released: send the cast order for the skill that was aimed.</summary>
    public void CommandSkillCast(KeyCode keyCode, bool addToCommandsQueue = false)
    {
        var index = IndexOfKey(keyCode);

        // A key coming up only fires what the same key prepared. A skill aimed
        // from a button in the meantime is left alone.
        if (_preparedByClick || index < 0 || index != _preparedIndex)
        {
            if (!_preparedByClick)
            {
                ClearPrepared();
            }

            return;
        }

        Fire(addToCommandsQueue);
    }

    /// <summary>
    /// Is there a skill on this key at all, never mind whether it can be cast
    /// right now? Asked by the input to tell a held skill key from a held key
    /// that means nothing, so that a cast can fire the moment its cooldown ends
    /// (T-023).
    /// </summary>
    public bool HasSkillOnKey(KeyCode keyCode)
    {
        return IndexOfKey(keyCode) >= 0;
    }

    // --- buttons ----------------------------------------------------------------

    /// <summary>
    /// A skill button was clicked: aim it, as a pressed key would. Nothing
    /// happens when it cannot be cast right now — no hint is shown either (Q-10).
    /// </summary>
    public bool PrepareSkillCastByClick(int index)
    {
        return Prepare(index, true);
    }

    /// <summary>Left click in the world while a button-aimed skill waits.</summary>
    public void CommandAimedSkillCast(bool addToCommandsQueue)
    {
        if (!IsAimingByClick)
        {
            return;
        }

        Fire(addToCommandsQueue);
    }

    /// <summary>Right click or Esc: the aimed skill is put away, nothing is cast.</summary>
    public void CancelAiming()
    {
        if (_preparedSkill == null)
        {
            return;
        }

        ClearPrepared();
    }

    // --- the cast itself --------------------------------------------------------

    private bool Prepare(int index, bool byClick)
    {
        if (!TryGetSkillCaster(index, out var unitToCast, out var unitSkill, out _))
            return false;

        // Nothing to aim at: the press is the cast (T-053). Aiming is not
        // touched, so a skill aimed by a button in the meantime stays aimed.
        if ((unitSkill.Skill as ActiveSkill).Action is CastWithoutTargetAction)
        {
            return CastAtOnce(index);
        }

        _preparedSkill = unitSkill;
        _preparedIndex = index;
        _preparedByClick = byClick;

        // Getting here at all means the cast is possible right now, which is
        // exactly when the hints are allowed to show (M-020).
        _aimHints?.Show(unitToCast, unitSkill.Skill as ActiveSkill);

        _commands?.NotifySkillAimingChanged();
        return true;
    }

    private void Fire(bool addToCommandsQueue)
    {
        var index = _preparedIndex;
        var prepared = _preparedSkill;

        // The aiming is over whatever comes of the cast itself.
        ClearPrepared();

        if (!TryGetSkillCaster(index, out var unitToCast, out var unitSkill, out var unitSkillsScript))
            return;

        if (prepared != unitSkill)
            return;

        var skillCastArgs = unitSkillsScript.CreateCommandArgs(unitSkill, addToCommandsQueue);

        // Nothing to cast at — the cast is cancelled and no order is given.
        if (skillCastArgs == null)
        {
            return;
        }

        unitToCast.GetComponent<UnitEventManager>().OnSkillCastCommandReceived(skillCastArgs);
    }

    /// <summary>
    /// A skill without a target goes off right now, past the order queue: the
    /// unit keeps doing what it does (T-053). The first selected caster of the
    /// kind that can cast it and is not stunned — a stunned unit takes orders
    /// for later, but an instant cast has no later.
    /// </summary>
    private bool CastAtOnce(int index)
    {
        var firstSkillsScript = MainCasterSkills();
        if (firstSkillsScript == null)
            return false;

        var unitId = firstSkillsScript.GetComponent<UnitValues>().Id;

        foreach (var caster in _unitsController.SelectedUnits.Where(x => x.GetComponent<UnitValues>().Id == unitId))
        {
            var skillsScript = caster.GetComponent<UnitSkills>();
            if (skillsScript == null || index >= skillsScript.RuntimeSkills.Count)
                continue;

            var skill = skillsScript.RuntimeSkills[index];
            if (!skillsScript.CheckIfCanCast(skill))
                continue;

            var effects = caster.GetComponent<UnitEffects>();
            if (effects != null && effects.HasAny<StunEffect>())
                continue;

            skillsScript.Cast(skill, new SkillParams { Owner = caster });
            return true;
        }

        return false;
    }

    private void ClearPrepared()
    {
        var hadSkill = _preparedSkill != null;

        _aimHints?.Hide();
        _preparedSkill = null;
        _preparedIndex = -1;
        _preparedByClick = false;

        if (hadSkill)
        {
            _commands?.NotifySkillAimingChanged();
        }
    }

    private void OnSelectionChanged(SelectionChangedEventArgs args)
    {
        // Aimed with somebody who is no longer selected: nothing to aim with.
        CancelAiming();
    }

    // --- who casts --------------------------------------------------------------

    /// <summary>Place of the skill on this key in the main unit's list, -1 when none.</summary>
    public int IndexOfKey(KeyCode keyCode)
    {
        var skills = MainCasterSkills();
        var skill = skills?.GetSkillByKeycode(keyCode);

        if (skill == null)
            return -1;

        for (var i = 0; i < skills.RuntimeSkills.Count; i++)
        {
            if (skills.RuntimeSkills[i] == skill)
                return i;
        }

        return -1;
    }

    /// <summary>Skills of the main selected unit if it is ours and allowed to cast.</summary>
    private UnitSkills MainCasterSkills()
    {
        var selectedUnits = _unitsController.SelectedUnits;

        if (_unitsController.SelectedUnitsTeamId != _playerTeamId || !selectedUnits.Any())
            return null;

        var firstUnit = selectedUnits.First();
        var firstValues = firstUnit.GetComponent<UnitValues>();

        if (firstValues == null || !firstValues.CanCastSkills)
            return null;

        return firstUnit.GetComponent<UnitSkills>();
    }

    /// <summary>
    /// Of the selected casters of the same kind, the first one that can cast
    /// the skill at this place right now.
    /// </summary>
    private bool TryGetSkillCaster(int index, out GameObject unitToCast, out UnitSkill unitSkill, out UnitSkills unitSkillsScript)
    {
        unitToCast = null;
        unitSkill = null;
        unitSkillsScript = null;

        var firstSkillsScript = MainCasterSkills();

        if (firstSkillsScript == null || index < 0 || index >= firstSkillsScript.RuntimeSkills.Count)
            return false;

        if (firstSkillsScript.RuntimeSkills[index].Skill is not ActiveSkill)
            return false;

        var unitId = firstSkillsScript.GetComponent<UnitValues>().Id;

        foreach (var caster in _unitsController.SelectedUnits.Where(x => x.GetComponent<UnitValues>().Id == unitId))
        {
            var skillsScript = caster.GetComponent<UnitSkills>();

            if (skillsScript == null || index >= skillsScript.RuntimeSkills.Count)
                continue;

            var skill = skillsScript.RuntimeSkills[index];

            if (skillsScript.CheckIfCanCast(skill))
            {
                unitToCast = caster;
                unitSkill = skill;
                unitSkillsScript = skillsScript;
                return true;
            }
        }

        return false;
    }
}
