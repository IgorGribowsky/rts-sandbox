using UnityEngine;

public class InstantDamageExecutor : SkillImpactExecutor<InstantDamageImpact>
{
    public InstantDamageExecutor(InstantDamageImpact data) : base(data) { }

    public override void ApplyToUnit(GameObject target, GameObject skillOwner)
    {
        target.GetComponent<UnitEventManager>().OnDamageReceived(
            attacker: skillOwner,
            damageAmount: Data.damage,
            damageType: Data.type);
    }
}
