using NUnit.Framework;
using TenRush.Core.Model;

namespace TenRush.Tests
{
    public class TileGridTests
    {
        [Test]
        public void HasValidPair_ReturnsTrue_WhenSomePairSumsToTen()
        {
            var grid = new TileGrid(2, 2);
            grid.LoadFrom(new[,] { { 3, 7 }, { 2, 2 } });

            Assert.IsTrue(grid.HasValidPair());
        }

        [Test]
        public void HasValidPair_ReturnsFalse_WhenNoPairSumsToTen()
        {
            var grid = new TileGrid(2, 2);
            grid.LoadFrom(new[,] { { 1, 1 }, { 2, 2 } });

            Assert.IsFalse(grid.HasValidPair());
        }

        [Test]
        public void Indexer_ReadsLoadedValues()
        {
            var grid = new TileGrid(2, 2);
            grid.LoadFrom(new[,] { { 9, 8 }, { 7, 6 } });

            Assert.AreEqual(9, grid[0, 0]);
            Assert.AreEqual(6, grid[new GridPosition(1, 1)]);
        }
    }
}
