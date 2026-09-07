using UnityEngine;

[CreateAssetMenu(fileName = "NewThrowWaveProjectileAction", menuName = "Game/SkillActions/Throw Wave Projectile Action")]
public class ThrowWaveProjectileAction : ThrowProjectileAction
{
    public float WaveWidthScale = 1f;

    public override SkillActionType Type => SkillActionType.ThrowWaveProjectile;
}
