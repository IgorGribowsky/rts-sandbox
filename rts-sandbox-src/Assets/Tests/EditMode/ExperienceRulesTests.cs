using NUnit.Framework;
using RtsSandbox.Rules;

namespace RtsSandbox.Rules.Tests
{
    /// <summary>
    /// Numbers from M-026: thresholds 100, 200, 300 … up to level 10, the reward
    /// shared evenly among everyone with levels in the radius.
    /// </summary>
    public class ExperienceRulesTests
    {
        // 100, 200, …, 900: nine level-ups, levels 1 to 10.
        private static readonly int[] DocTable = { 100, 200, 300, 400, 500, 600, 700, 800, 900 };

        [Test]
        public void MaxLevel_IsTen_ForTheDocTable()
        {
            Assert.AreEqual(10, ExperienceRules.MaxLevel(DocTable));
        }

        [Test]
        public void Add_BelowThreshold_StaysOnLevel()
        {
            var state = ExperienceRules.Add(new LevelState(1, 0), 40, DocTable);

            Assert.AreEqual(1, state.Level);
            Assert.AreEqual(40, state.Experience);
        }

        [Test]
        public void Add_ExactlyThreshold_LevelsUpWithEmptyBar()
        {
            // "Набрать 100 — уровень 2, опыт 0/200".
            var state = ExperienceRules.Add(new LevelState(1, 0), 100, DocTable);

            Assert.AreEqual(2, state.Level);
            Assert.AreEqual(0, state.Experience);
            Assert.AreEqual(200, ExperienceRules.CostOfNextLevel(state.Level, DocTable));
        }

        [Test]
        public void Add_BigGain_ClimbsSeveralLevels_AndKeepsTheRest()
        {
            // 100 to reach 2, 200 to reach 3, 50 left over.
            var state = ExperienceRules.Add(new LevelState(1, 0), 350, DocTable);

            Assert.AreEqual(3, state.Level);
            Assert.AreEqual(50, state.Experience);
        }

        [Test]
        public void Add_AtTopLevel_GathersNothing()
        {
            var state = ExperienceRules.Add(new LevelState(10, 0), 500, DocTable);

            Assert.AreEqual(10, state.Level);
            Assert.AreEqual(0, state.Experience);
            Assert.AreEqual(0, ExperienceRules.CostOfNextLevel(10, DocTable));
        }

        [Test]
        public void Add_ReachingTopLevel_StopsThere()
        {
            var state = ExperienceRules.Add(new LevelState(9, 850), 1000, DocTable);

            Assert.AreEqual(10, state.Level);
            Assert.AreEqual(0, state.Experience);
        }

        [Test]
        public void Share_SplitsEvenly()
        {
            // "Два Caster рядом убивают врага — каждый получил половину".
            Assert.AreEqual(30, ExperienceRules.Share(60, 2));
            Assert.AreEqual(60, ExperienceRules.Share(60, 1));
        }

        [Test]
        public void Share_UnevenReward_RoundsDown()
        {
            Assert.AreEqual(16, ExperienceRules.Share(50, 3));
        }

        [Test]
        public void Share_NobodyToShareWith_GivesNothing()
        {
            Assert.AreEqual(0, ExperienceRules.Share(60, 0));
            Assert.AreEqual(0, ExperienceRules.Share(0, 2));
        }
    }
}
