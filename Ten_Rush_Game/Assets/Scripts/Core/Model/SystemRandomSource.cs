using System;

namespace TenRush.Core.Model
{
    /// <summary>System.Random 기반 기본 랜덤 소스. 실제 플레이에서 사용.</summary>
    public sealed class SystemRandomSource : IRandomSource
    {
        private readonly Random _random;

        public SystemRandomSource() : this(new Random())
        {
        }

        public SystemRandomSource(int seed) : this(new Random(seed))
        {
        }

        private SystemRandomSource(Random random)
        {
            _random = random;
        }

        public int NextTileValue()
        {
            return GridConstants.MinTileValue +
                   _random.Next(GridConstants.MaxTileValue - GridConstants.MinTileValue + 1);
        }
    }
}
