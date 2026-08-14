namespace TenRush.Core.Model
{
    /// <summary>
    /// <see cref="GameRound.Tap"/> 호출 결과. <see cref="Outcome"/>에 따라 의미 있는 필드만 채워진다
    /// (예: Matched일 때만 MatchedA/MatchedB/ScoreGained/ComboCount가 유효).
    /// </summary>
    public readonly struct TapResult
    {
        public readonly TapOutcome Outcome;

        public readonly GridPosition? Selected;
        public readonly GridPosition? Deselected;

        public readonly GridPosition MatchedA;
        public readonly GridPosition MatchedB;
        public readonly int ScoreGained;
        public readonly int ComboCount;

        public readonly GridPosition MismatchPrevious;
        public readonly GridPosition MismatchNewSelection;

        private TapResult(
            TapOutcome outcome,
            GridPosition? selected,
            GridPosition? deselected,
            GridPosition matchedA,
            GridPosition matchedB,
            int scoreGained,
            int comboCount,
            GridPosition mismatchPrevious,
            GridPosition mismatchNewSelection)
        {
            Outcome = outcome;
            Selected = selected;
            Deselected = deselected;
            MatchedA = matchedA;
            MatchedB = matchedB;
            ScoreGained = scoreGained;
            ComboCount = comboCount;
            MismatchPrevious = mismatchPrevious;
            MismatchNewSelection = mismatchNewSelection;
        }

        internal static TapResult CreateIgnored() =>
            new TapResult(TapOutcome.Ignored, null, null, default, default, 0, 0, default, default);

        internal static TapResult CreateSelected(GridPosition pos) =>
            new TapResult(TapOutcome.Selected, pos, null, default, default, 0, 0, default, default);

        internal static TapResult CreateDeselected(GridPosition pos) =>
            new TapResult(TapOutcome.Deselected, null, pos, default, default, 0, 0, default, default);

        internal static TapResult CreateMatched(GridPosition a, GridPosition b, int scoreGained, int comboCount) =>
            new TapResult(TapOutcome.Matched, null, null, a, b, scoreGained, comboCount, default, default);

        internal static TapResult CreateMismatched(GridPosition previous, GridPosition newSelection) =>
            new TapResult(TapOutcome.Mismatched, newSelection, null, default, default, 0, 0, previous, newSelection);
    }
}
