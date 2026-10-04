using System;
using System.Collections.Generic;

namespace RtsSandbox.Rules
{
    /// <summary>One source of critical strikes on a unit: a chance and what the blow is multiplied by.</summary>
    public readonly struct CritChance
    {
        public CritChance(float chancePercent, float multiplier)
        {
            ChancePercent = chancePercent;
            Multiplier = multiplier;
        }

        /// <summary>0..100.</summary>
        public float ChancePercent { get; }

        public float Multiplier { get; }
    }

    /// <summary>
    /// Critical strike on an ordinary attack (M-007, M-015, T-070). Pure
    /// arithmetic: the dice come in as a function, so the rule is checked
    /// without chance.
    ///
    /// Every source rolls on its own. Of those that came up, the biggest
    /// multiplier counts — two crits never multiply each other, as in WC3.
    /// </summary>
    public static class CriticalStrikeRules
    {
        /// <param name="roll01">Gives 0..1, one call per source; 0.1 comes up under a chance of 15%.</param>
        public static float Resolve(float damage, IReadOnlyList<CritChance> sources, Func<float> roll01, out bool critical)
        {
            critical = false;

            if (sources == null || sources.Count == 0 || roll01 == null)
            {
                return damage;
            }

            var best = 1f;

            foreach (var source in sources)
            {
                var roll = roll01();
                if (roll * 100f < source.ChancePercent && source.Multiplier > best)
                {
                    best = source.Multiplier;
                }
            }

            critical = best > 1f;
            return damage * best;
        }
    }
}
