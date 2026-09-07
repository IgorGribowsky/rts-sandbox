using Assets.SkillsSection.Scripts;
using Assets.SkillsSection.Scripts.Aiming;
using System.Linq;
using UnityEngine;
using static Assets.SkillsSection.Scripts.UnitSkills;

/// <summary>
/// Aiming and casting a skill with the current selection. Lives apart from
/// UnitsController on purpose: selecting units and giving orders should not
/// know anything about how a skill is aimed.
/// </summary>
public class SkillController : MonoBehaviour
{
    private UnitsController _unitsController;

    private int _playerTeamId;

    private UnitSkill _preparedSkill = null;

    /// <summary>Optional: without it aiming simply shows nothing (M-020).</summary>
    private SkillAimHintController _aimHints;

    void Start()
    {
        _unitsController = GetComponent<UnitsController>();
        _playerTeamId = GetComponent<PlayerTeamMember>().TeamId;
        _aimHints = GetComponent<SkillAimHintController>();
    }

    /// <summary>Key pressed: remember which skill of which caster is being aimed.</summary>
    public bool PrepareSkillCast(KeyCode keyCode)
    {
        if (!TryGetSkillCaster(keyCode, out var unitToCast, out var unitSkill, out _))
            return false;

        _preparedSkill = unitSkill;

        // Getting here at all means the cast is possible right now, which is
        // exactly when the hints are allowed to show (M-020).
        _aimHints?.Show(unitToCast, unitSkill.Skill as ActiveSkill);

        return true;
    }

    /// <summary>Key released: send the cast order for the skill that was aimed.</summary>
    public void CommandSkillCast(KeyCode keyCode, bool addToCommandsQueue = false)
    {
        // The key is up, so the aiming is over whatever comes of the cast itself.
        _aimHints?.Hide();

        if (!TryGetSkillCaster(keyCode, out var unitToCast, out var unitSkill, out var unitSkillsScript))
            return;

        if (_preparedSkill != unitSkill)
            return;

        var skillCastArgs = unitSkillsScript.CreateCommandArgs(unitSkill, addToCommandsQueue);

        // The aiming is over either way, so the skill stops being the prepared one.
        _preparedSkill = null;

        // Nothing to cast at — the cast is cancelled and no order is given.
        if (skillCastArgs == null)
        {
            return;
        }

        unitToCast.GetComponent<UnitEventManager>().OnSkillCastCommandReceived(skillCastArgs);
    }

    /// <summary>
    /// Is there a skill on this key at all, never mind whether it can be cast
    /// right now? Asked by the input to tell a held skill key from a held key
    /// that means nothing, so that a cast can fire the moment its cooldown ends
    /// (T-023).
    /// </summary>
    public bool HasSkillOnKey(KeyCode keyCode)
    {
        var selectedUnits = _unitsController.SelectedUnits;

        if (_unitsController.SelectedUnitsTeamId != _playerTeamId || !selectedUnits.Any())
            return false;

        var firstUnit = selectedUnits.First();
        var firstValues = firstUnit.GetComponent<UnitValues>();

        if (firstValues == null || !firstValues.CanCastSkills)
            return false;

        return firstUnit.GetComponent<UnitSkills>()?.GetSkillByKeycode(keyCode) != null;
    }

    /// <summary>
    /// Of the selected casters of the same kind, the first one that can cast
    /// this key right now.
    /// </summary>
    private bool TryGetSkillCaster(KeyCode keyCode, out GameObject unitToCast, out UnitSkill unitSkill, out UnitSkills unitSkillsScript)
    {
        unitToCast = null;
        unitSkill = null;
        unitSkillsScript = null;

        var selectedUnits = _unitsController.SelectedUnits;

        if (_unitsController.SelectedUnitsTeamId != _playerTeamId || !selectedUnits.Any())
            return false;

        var firstUnit = selectedUnits.First();
        var firstValues = firstUnit.GetComponent<UnitValues>();

        if (firstValues == null || !firstValues.CanCastSkills)
            return false;

        var firstSkillsScript = firstUnit.GetComponent<UnitSkills>();
        var firstSkill = firstSkillsScript?.GetSkillByKeycode(keyCode);

        if (firstSkill == null)
            return false;

        var unitId = firstValues.Id;

        foreach (var caster in selectedUnits.Where(x => x.GetComponent<UnitValues>().Id == unitId))
        {
            var skillsScript = caster.GetComponent<UnitSkills>();
            var skill = skillsScript.GetSkillByKeycode(keyCode);

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
