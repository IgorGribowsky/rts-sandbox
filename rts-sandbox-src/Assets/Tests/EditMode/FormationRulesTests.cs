using NUnit.Framework;
using RtsSandbox.Rules;

namespace RtsSandbox.Rules.Tests
{
    /// <summary>
    /// Order from M-003 and T-034: melee in front of ranged, inside each the
    /// higher Rang in front.
    /// </summary>
    public class FormationRulesTests
    {
        [Test]
        public void Arrange_MeleeStandsInFrontOfRanged()
        {
            var places = FormationRules.Arrange(new[]
            {
                new FormationMember(false, 5, 1),
                new FormationMember(true, 1, 2),
            });

            Assert.Greater(places[1].Forward, places[0].Forward);
        }

        [Test]
        public void Arrange_HigherRangStandsInFront_InsideMelee()
        {
            // Five melee make rows of three. The strong one is listed last and
            // still lands in the front row; two of the equal ones go back.
            var places = FormationRules.Arrange(new[]
            {
                new FormationMember(true, 1, 1),
                new FormationMember(true, 1, 2),
                new FormationMember(true, 1, 3),
                new FormationMember(true, 1, 4),
                new FormationMember(true, 3, 5),
            });

            Assert.AreEqual(places[0].Forward, places[4].Forward);
            Assert.Greater(places[4].Forward, places[2].Forward);
            Assert.Greater(places[4].Forward, places[3].Forward);
        }

        [Test]
        public void Arrange_RangedNeverShareARowWithMelee()
        {
            // Rows of two: the third melee unit leaves a gap in the second row,
            // and the archer must not fill it.
            var places = FormationRules.Arrange(new[]
            {
                new FormationMember(true, 1, 1),
                new FormationMember(true, 1, 2),
                new FormationMember(true, 1, 3),
                new FormationMember(false, 1, 4),
            });

            Assert.Less(places[3].Forward, places[0].Forward);
            Assert.Less(places[3].Forward, places[1].Forward);
            Assert.Less(places[3].Forward, places[2].Forward);
        }

        [Test]
        public void Arrange_BlockIsCentredOnThePoint()
        {
            var places = FormationRules.Arrange(new[]
            {
                new FormationMember(true, 1, 1),
                new FormationMember(true, 1, 2),
                new FormationMember(false, 1, 3),
                new FormationMember(false, 1, 4),
            });

            float right = 0, forward = 0;
            foreach (var place in places)
            {
                right += place.Right;
                forward += place.Forward;
            }

            Assert.AreEqual(0f, right, 0.0001f);
            Assert.AreEqual(0f, forward, 0.0001f);
        }

        [Test]
        public void Arrange_InsideARow_LeftUnitTakesLeftPlace()
        {
            var places = FormationRules.Arrange(new[]
            {
                new FormationMember(true, 1, 1, side: 5f),
                new FormationMember(true, 1, 2, side: -5f),
            });

            Assert.Less(places[1].Right, places[0].Right);
        }

        [Test]
        public void Arrange_Empty_GivesNothing()
        {
            Assert.AreEqual(0, FormationRules.Arrange(new FormationMember[0]).Length);
        }

        [Test]
        public void Arrange_OneUnit_StandsOnThePoint()
        {
            var places = FormationRules.Arrange(new[] { new FormationMember(false, 1, 1) });

            Assert.AreEqual(0f, places[0].Right);
            Assert.AreEqual(0f, places[0].Forward);
        }
    }
}
