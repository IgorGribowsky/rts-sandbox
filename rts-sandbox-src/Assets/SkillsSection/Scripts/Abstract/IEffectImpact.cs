/// <summary>
/// An impact that leaves an effect hanging on the unit (stun, poison, boost)
/// and may say how that effect looks in the HUD (T-073). The effect is keyed
/// by the impact data, so the HUD gets back here from the effect's Key.
/// </summary>
public interface IEffectImpact
{
    EffectInfo EffectInfo { get; }
}
