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
    }
}
