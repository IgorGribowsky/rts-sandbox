using UnityEngine;

/// <summary>
/// The mana pool of one unit. Sits next to UnitManaPoints, which spends and
/// regenerates it. A unit without mana has neither component (T-015).
/// </summary>
public class ManaValues : MonoBehaviour
{
    [Tooltip("This unit's own copy of the type numbers.")]
    public ManaStats Stats = new ManaStats();

    public float CurrentMana { get => Stats.CurrentMana; set => Stats.CurrentMana = value; }

    public float MaximumMana { get => Stats.MaximumMana; set => Stats.MaximumMana = value; }

    public float BaseManaRegen { get => Stats.BaseManaRegen; set => Stats.BaseManaRegen = value; }
}
