using Assets.SkillsSection.Scripts;
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

    void Start()
    {
        _unitsController = GetComponent<UnitsController>();
        _playerTeamId = GetComponent<PlayerTeamMember>().TeamId;
    }

    /// <summary>Key pressed: remember which skill of which caster is being aimed.</summary>
    public bool PrepareSkillCast(KeyCode keyCode)
    {
        if (!TryGetSkillCaster(keyCode, out _, out var unitSkill, out _))
            return false;

        _preparedSkill = unitSkill;
        return true;
    }

    /// <summary>Key released: send the cast order for the skill that was aimed.</summary>
    public void CommandSkillCast(KeyCode keyCode, bool addToCommandsQueue = false)
    {
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
