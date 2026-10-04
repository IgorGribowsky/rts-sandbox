using System;
using System.Collections.Generic;

namespace RtsSandbox.Rules
{
    /// <summary>A level and the experience gathered towards the next one.</summary>
    public readonly struct LevelState
    {
        public LevelState(int level, int experience)
        {
            Level = level;
            Experience = experience;
        }

        public int Level { get; }

        /// <summary>Experience inside the current level, from 0 up to the threshold.</summary>
        public int Experience { get; }
    }

    /// <summary>
    /// Experience and levels (M-026). Pure arithmetic: the table and the numbers
    /// come in as values, so the rules are checked without a scene.
    ///
    /// The table lists what each level-up costs: entry 0 takes level 1 to 2,
    /// entry 1 takes 2 to 3, and so on. Its length sets the top level.
    /// </summary>
    public static class ExperienceRules
    {
        public const int FirstLevel = 1;

        public static int MaxLevel(IReadOnlyList<int> levelUpCosts)
        {
            return FirstLevel + (levelUpCosts?.Count ?? 0);
        }

        /// <summary>Experience needed to leave this level; 0 at the top level.</summary>
        public static int CostOfNextLevel(int level, IReadOnlyList<int> levelUpCosts)
        {
            if (level >= MaxLevel(levelUpCosts) || level < FirstLevel)
            {
                return 0;
            }

            return Math.Max(1, levelUpCosts[level - FirstLevel]);
        }

        /// <summary>
        /// What each of the units sharing a kill gets: the reward split evenly,
        /// rounded down. Nobody to share with — nobody gets anything.
        /// </summary>
        public static int Share(int reward, int receivers)
        {
            if (reward <= 0 || receivers <= 0)
            {
                return 0;
            }

            return reward / receivers;
        }

        /// <summary>
        /// Adds experience, climbing as many levels as it pays for. What is left
        /// over after a level-up counts towards the next one. At the top level
        /// experience is no longer gathered.
        /// </summary>
        public static LevelState Add(LevelState state, int gain, IReadOnlyList<int> levelUpCosts)
        {
            var level = Math.Max(FirstLevel, state.Level);
            var experience = Math.Max(0, state.Experience);
            var maxLevel = MaxLevel(levelUpCosts);

            if (gain > 0)
            {
                experience += gain;
            }

            while (level < maxLevel)
            {
                var cost = CostOfNextLevel(level, levelUpCosts);
                if (experience < cost)
                {
                    break;
                }

                experience -= cost;
                level++;
            }

            if (level >= maxLevel)
            {
                level = maxLevel;
                experience = 0;
            }

            return new LevelState(level, experience);
        }
    }
}
