using NUnit.Framework;
using RtsSandbox.Rules;

namespace RtsSandbox.Rules.Tests
{
    /// <summary>
    /// Three states from M-027: black where never seen, grey where seen
    /// before, clear where seen now. Seen once, never black again.
    /// </summary>
    public class FogRulesTests
    {
        [Test]
        public void NewGrid_IsAllUnexplored()
        {
            var grid = new FogGrid(4, 3);

            foreach (var cell in grid.Cells)
            {
                Assert.AreEqual(FogState.Unexplored, cell);
            }
        }

        [Test]
        public void Reveal_MakesCellsInsideRadiusVisible_AndLeavesTheRestBlack()
        {
            var grid = new FogGrid(20, 20);

            grid.BeginPass();
            grid.Reveal(10f, 10f, 3f);

            Assert.AreEqual(FogState.Visible, grid[10, 10]);
            Assert.AreEqual(FogState.Visible, grid[12, 10]);
            Assert.AreEqual(FogState.Unexplored, grid[14, 10]);
            Assert.AreEqual(FogState.Unexplored, grid[0, 0]);
        }

        [Test]
        public void NextPass_WithoutSight_TurnsVisibleGrey_NotBlack()
        {
            var grid = new FogGrid(20, 20);
            grid.BeginPass();
            grid.Reveal(10f, 10f, 3f);

            grid.BeginPass();

            Assert.AreEqual(FogState.Explored, grid[10, 10]);
            Assert.AreEqual(FogState.Unexplored, grid[14, 10]);
        }

        [Test]
        public void Explored_StaysExplored_UntilSeenAgain()
        {
            var grid = new FogGrid(20, 20);
            grid.BeginPass();
            grid.Reveal(5f, 5f, 2f);

            // The unit walks away: many passes, the old place is never black again.
            for (var i = 0; i < 5; i++)
            {
                grid.BeginPass();
                grid.Reveal(15f, 15f, 2f);
            }

            Assert.AreEqual(FogState.Explored, grid[5, 5]);

            grid.BeginPass();
            grid.Reveal(5f, 5f, 2f);

            Assert.AreEqual(FogState.Visible, grid[5, 5]);
            Assert.AreEqual(FogState.Explored, grid[15, 15]);
        }

        [Test]
        public void Reveal_NearOrOffTheEdge_RevealsOnlyThePartOnTheGrid()
        {
            var grid = new FogGrid(10, 10);

            grid.BeginPass();
            grid.Reveal(-1f, -1f, 3f);
            grid.Reveal(12f, 5f, 1f);

            Assert.AreEqual(FogState.Visible, grid[0, 0]);
            Assert.AreEqual(FogState.Unexplored, grid[9, 5]);
        }

        [Test]
        public void Reveal_WithZeroRadius_RevealsNothing()
        {
            var grid = new FogGrid(10, 10);

            grid.BeginPass();
            grid.Reveal(5f, 5f, 0f);

            Assert.AreEqual(FogState.Unexplored, grid[5, 5]);
        }

        [Test]
        public void Light_IsFullInside_FadesOverFeather_AndIsZeroOutside()
        {
            var grid = new FogGrid(20, 20);

            grid.BeginPass();
            grid.Reveal(10.5f, 10.5f, 6f, 2f);

            Assert.AreEqual(1f, grid.Light[10 * 20 + 10]);
            // Centre of cell (15, 10) is 5 away: half way into the 2-cell feather.
            Assert.AreEqual(0.5f, grid.Light[10 * 20 + 15], 1e-4f);
            Assert.AreEqual(FogState.Visible, grid[15, 10]);
            Assert.AreEqual(0f, grid.Light[10 * 20 + 17]);
        }

        [Test]
        public void Light_OfOverlappingCircles_IsTheBrighter_AndNextPassClearsIt()
        {
            var grid = new FogGrid(20, 20);

            grid.BeginPass();
            grid.Reveal(10f, 10f, 6f, 2f);
            grid.Reveal(15f, 10f, 3f, 1f);

            Assert.AreEqual(1f, grid.Light[10 * 20 + 15]);

            grid.BeginPass();

            Assert.AreEqual(0f, grid.Light[10 * 20 + 15]);
            Assert.AreEqual(FogState.Explored, grid[15, 10]);
        }

        [Test]
        public void SightOf_Friendly_IsAlwaysVisible_EvenInBlack()
        {
            Assert.AreEqual(FogSight.Visible, FogRules.SightOf(true, false, false, false));
            Assert.AreEqual(FogSight.Visible, FogRules.SightOf(true, true, false, false));
        }

        [Test]
        public void SightOf_EnemyUnit_IsVisibleInSight_AndHiddenOutOfIt_EvenIfSeenBefore()
        {
            Assert.AreEqual(FogSight.Visible, FogRules.SightOf(false, false, true, false));
            Assert.AreEqual(FogSight.Hidden, FogRules.SightOf(false, false, false, true));
        }

        [Test]
        public void SightOf_EnemyBuilding_SeenBefore_IsRemembered_NeverSeen_IsHidden()
        {
            Assert.AreEqual(FogSight.Remembered, FogRules.SightOf(false, true, false, true));
            Assert.AreEqual(FogSight.Hidden, FogRules.SightOf(false, true, false, false));
            Assert.AreEqual(FogSight.Visible, FogRules.SightOf(false, true, true, true));
        }

        // --- trees block sight (T-071.2, M-027: "за лесом не видно") ---------

        [Test]
        public void Blocker_HidesWhatIsBehindIt_ButIsSeenItself()
        {
            var grid = new FogGrid(30, 30);
            for (var y = 13; y <= 17; y++)
            {
                grid.SetBlocker(10, y);
            }

            grid.BeginPass();
            grid.Reveal(5.5f, 15.5f, 12f);

            Assert.AreEqual(FogState.Visible, grid[10, 15]);
            Assert.AreEqual(FogState.Unexplored, grid[14, 15]);
            Assert.AreEqual(FogState.Visible, grid[9, 15]);
        }

        [Test]
        public void Blocker_DoesNotHide_WhatIsBesideIt_AlongTheForestEdge()
        {
            var grid = new FogGrid(30, 30);
            for (var x = 4; x <= 25; x++)
            {
                grid.SetBlocker(x, 10);
            }

            grid.BeginPass();
            grid.Reveal(5.5f, 11.5f, 12f);

            // Along the edge of the forest, on the viewer's side: seen.
            Assert.AreEqual(FogState.Visible, grid[15, 11]);
            // The trees of the edge: seen. Behind them: not.
            Assert.AreEqual(FogState.Visible, grid[12, 10]);
            Assert.AreEqual(FogState.Unexplored, grid[12, 8]);
        }

        [Test]
        public void Blocker_UnderTheViewer_DoesNotBlindIt()
        {
            var grid = new FogGrid(20, 20);
            grid.SetBlocker(10, 10);

            grid.BeginPass();
            grid.Reveal(10.5f, 10.5f, 5f);

            Assert.AreEqual(FogState.Visible, grid[14, 10]);
            Assert.AreEqual(FogState.Visible, grid[10, 6]);
        }

        [Test]
        public void ClearBlockers_OpensTheViewAgain_OnTheNextPass()
        {
            var grid = new FogGrid(30, 30);
            grid.SetBlocker(10, 15);
            grid.BeginPass();
            grid.Reveal(5.5f, 15.5f, 12f);
            Assert.AreEqual(FogState.Unexplored, grid[14, 15]);

            // The tree is cut down.
            grid.ClearBlockers();
            grid.BeginPass();
            grid.Reveal(5.5f, 15.5f, 12f);

            Assert.AreEqual(FogState.Visible, grid[14, 15]);
        }

        [Test]
        public void Blocker_OutsideTheCircle_ChangesNothing()
        {
            var open = new FogGrid(40, 40);
            var withTree = new FogGrid(40, 40);
            withTree.SetBlocker(35, 35);

            open.BeginPass();
            open.Reveal(10.5f, 10.5f, 8f, 2f);
            withTree.BeginPass();
            withTree.Reveal(10.5f, 10.5f, 8f, 2f);

            CollectionAssert.AreEqual(open.Cells, withTree.Cells);
            CollectionAssert.AreEqual(open.Light, withTree.Light);
        }
    }
}
