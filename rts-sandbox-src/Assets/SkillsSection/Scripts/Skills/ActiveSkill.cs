using UnityEngine;

[CreateAssetMenu(fileName = "NewActiveSkill", menuName = "Game/Skills/Active Skill")]
public class ActiveSkill : Skill
{
    public float CastDuration;
    public float CastRange;
    public int ManaCost;
    public ActiveSkillAction Action;
}
