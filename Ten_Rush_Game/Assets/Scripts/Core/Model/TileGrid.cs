namespace TenRush.Core.Model
{
    /// <summary>
    /// 숫자 타일 보드. <c>int[,]</c> 하나가 게임 상태의 전부라는 기획서 8장 원칙을 그대로 따른다.
    /// 값을 바꾸는 경로는 <see cref="GameRound"/>로만 열어 두기 위해 쓰기는 internal로 잠근다.
    /// </summary>
    public sealed class TileGrid
    {
        private readonly int[,] _values;

        public int Rows { get; }
        public int Cols { get; }

        public TileGrid(int rows, int cols)
        {
            Rows = rows;
            Cols = cols;
            _values = new int[rows, cols];
        }

        public int this[int row, int col]
        {
            get => _values[row, col];
            internal set => _values[row, col] = value;
        }

        public int this[GridPosition pos]
        {
            get => _values[pos.Row, pos.Col];
            internal set => _values[pos.Row, pos.Col] = value;
        }

        public bool InBounds(int row, int col) => row >= 0 && row < Rows && col >= 0 && col < Cols;

        internal void FillRandom(IRandomSource random)
        {
            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Cols; c++)
                    _values[r, c] = random.NextTileValue();
        }

        /// <summary>테스트에서 정확한 보드 상태를 직접 구성할 때만 쓴다.</summary>
        internal void LoadFrom(int[,] source)
        {
            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Cols; c++)
                    _values[r, c] = source[r, c];
        }

        /// <summary>보드 전체에서 합이 MatchTarget(10)이 되는 페어가 하나라도 있는지 확인한다.</summary>
        public bool HasValidPair()
        {
            int count = Rows * Cols;
            var flat = new int[count];
            int i = 0;
            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Cols; c++)
                    flat[i++] = _values[r, c];

            for (int a = 0; a < count; a++)
                for (int b = a + 1; b < count; b++)
                    if (flat[a] + flat[b] == GridConstants.MatchTarget)
                        return true;

            return false;
        }
    }
}
