using NUnit.Framework;
using RtsSandbox.Rules;

namespace RtsSandbox.Rules.Tests
{
    /// <summary>
    /// Numbers from M-015 and T-070: by default 15% and ×2; at 100% every blow
    /// is a crit. Two crits on one unit do not multiply each other.
    /// </summary>
    public class CriticalStrikeRulesTests
    {
        private static readonly CritChance[] Default = { new CritChance(15f, 2f) };

        [Test]
        public void RollUnderChance_DoublesTheBlow()
        {
            var damage = CriticalStrikeRules.Resolve(9f, Default, () => 0.10f, out var critical);

            Assert.IsTrue(critical);
            Assert.AreEqual(18f, damage, 0.0001f);
        }

        [Test]
        public void RollOverChance_LeavesTheBlowAsItIs()
        {
            var damage = CriticalStrikeRules.Resolve(9f, Default, () => 0.20f, out var critical);

            Assert.IsFalse(critical);
            Assert.AreEqual(9f, damage, 0.0001f);
        }

        [Test]
        public void RollRightAtTheChance_IsNotACrit()
        {
            CriticalStrikeRules.Resolve(9f, Default, () => 0.15f, out var critical);

            Assert.IsFalse(critical);
        }

        [Test]
        public void HundredPercent_IsACritEvenOnTheHighestRoll()
        {
            var always = new[] { new CritChance(100f, 2f) };

            var damage = CriticalStrikeRules.Resolve(7f, always, () => 0.9999f, out var critical);

            Assert.IsTrue(critical);
            Assert.AreEqual(14f, damage, 0.0001f);
        }

        [Test]
        public void NoSources_NoCrit()
        {
            var damage = CriticalStrikeRules.Resolve(7f, new CritChance[0], () => 0f, out var critical);

            Assert.IsFalse(critical);
            Assert.AreEqual(7f, damage, 0.0001f);
        }

        [Test]
        public void TwoCritsComeUp_TheBiggerMultiplierCounts_NotTheProduct()
        {
            var two = new[] { new CritChance(50f, 2f), new CritChance(50f, 3f) };

            var damage = CriticalStrikeRules.Resolve(10f, two, () => 0f, out var critical);

            Assert.IsTrue(critical);
            Assert.AreEqual(30f, damage, 0.0001f);
        }

        [Test]
        public void EachSourceRollsOnItsOwn()
        {
            var two = new[] { new CritChance(15f, 2f), new CritChance(15f, 3f) };
            var rolls = new[] { 0.10f, 0.90f };
            var next = 0;

            var damage = CriticalStrikeRules.Resolve(10f, two, () => rolls[next++], out var critical);

            Assert.IsTrue(critical);
            Assert.AreEqual(20f, damage, 0.0001f);
            Assert.AreEqual(2, next);
        }
    }
}
