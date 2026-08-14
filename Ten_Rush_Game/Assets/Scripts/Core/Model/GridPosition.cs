using System;

namespace TenRush.Core.Model
{
    /// <summary>그리드 위 한 칸의 좌표(행, 열). 값 타입이라 비교/해시가 자동으로 성립한다.</summary>
    public readonly struct GridPosition : IEquatable<GridPosition>
    {
        public readonly int Row;
        public readonly int Col;

        public GridPosition(int row, int col)
        {
            Row = row;
            Col = col;
        }

        public bool Equals(GridPosition other) => Row == other.Row && Col == other.Col;
        public override bool Equals(object obj) => obj is GridPosition other && Equals(other);
        public override int GetHashCode() => (Row * 397) ^ Col;
        public override string ToString() => $"({Row}, {Col})";

        public static bool operator ==(GridPosition a, GridPosition b) => a.Equals(b);
        public static bool operator !=(GridPosition a, GridPosition b) => !a.Equals(b);
    }
}
