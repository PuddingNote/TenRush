using System.Collections.Generic;
using TenRush.Core.Model;

namespace TenRush.Tests
{
    /// <summary>정해진 값을 순서대로 내보내는 랜덤 소스. 큐가 비면 fallback 값을 계속 반환한다.</summary>
    internal sealed class FakeRandomSource : IRandomSource
    {
        private readonly Queue<int> _queued;
        private readonly int _fallback;

        public FakeRandomSource(IEnumerable<int> values, int fallback = 5)
        {
            _queued = new Queue<int>(values);
            _fallback = fallback;
        }

        public int NextTileValue() => _queued.Count > 0 ? _queued.Dequeue() : _fallback;

        /// <summary>이미 생성된 소스에 나중에 나올 값을 추가로 예약한다.</summary>
        public void Enqueue(params int[] values)
        {
            foreach (var value in values)
                _queued.Enqueue(value);
        }
    }
}
