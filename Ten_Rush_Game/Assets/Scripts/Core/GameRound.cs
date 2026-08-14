using TenRush.Core.Model;

namespace TenRush.Core
{
    /// <summary>
    /// 텐러쉬 한 판의 규칙 전부. UnityEngine을 전혀 모르므로 Unity 에디터 없이도
    /// dotnet test로 검증할 수 있다. 시간은 항상 호출자가 넘겨주는 값(now/delta)만
    /// 쓴다 — 벽시계에 의존하지 않아 테스트에서 완전히 결정론적으로 재현 가능하다.
    ///
    /// 매치 처리는 프로토타입(레퍼런스 JS)과 동일하게 2단계로 나뉜다:
    ///   1) Tap()에서 합 10을 확인하면 즉시 점수/콤보를 확정하고 Locked 상태로 들어간다
    ///      (그리드 값은 아직 안 바꿈 — UI가 제거 애니메이션을 보여줄 시간을 번다).
    ///   2) UI가 애니메이션을 끝낸 뒤 ResolvePendingMatch()를 호출하면 그제서야 두 칸을
    ///      새 값으로 채우고, 막힘 여부를 확인해 필요하면 전체 리셔플한다.
    /// </summary>
    public sealed class GameRound
    {
        private readonly IRandomSource _random;

        private GridPosition? _selected;
        private GridPosition? _pendingMatchA;
        private GridPosition? _pendingMatchB;

        private double _lastMatchAtMs = double.NegativeInfinity;
        private int _comboCount;

        public TileGrid Grid { get; }
        public int Score { get; private set; }
        public float TimeRemainingSeconds { get; private set; }
        public bool IsGameOver { get; private set; }

        /// <summary>매치 애니메이션이 끝나길 기다리는 동안 true. 이 동안의 탭은 전부 무시된다.</summary>
        public bool IsLocked { get; private set; }

        public GameRound(IRandomSource random = null)
        {
            _random = random ?? new SystemRandomSource();
            Grid = new TileGrid(GridConstants.Rows, GridConstants.Cols);
            Reset();
        }

        public void Reset()
        {
            Grid.FillRandom(_random);
            EnsurePlayable();

            _selected = null;
            _pendingMatchA = null;
            _pendingMatchB = null;
            _lastMatchAtMs = double.NegativeInfinity;
            _comboCount = 0;

            Score = 0;
            TimeRemainingSeconds = GridConstants.RoundTimeSeconds;
            IsGameOver = false;
            IsLocked = false;
        }

        /// <param name="nowMs">콤보 판정에 쓰는 현재 시각(ms). 호출자가 일관된 시계(예: Time.unscaledTime*1000)를 넘겨야 한다.</param>
        public TapResult Tap(int row, int col, double nowMs)
        {
            if (IsGameOver || IsLocked)
                return TapResult.CreateIgnored();

            var pos = new GridPosition(row, col);

            if (_selected.HasValue && _selected.Value == pos)
            {
                _selected = null;
                return TapResult.CreateDeselected(pos);
            }

            if (!_selected.HasValue)
            {
                _selected = pos;
                return TapResult.CreateSelected(pos);
            }

            var previous = _selected.Value;
            int sum = Grid[previous] + Grid[pos];

            if (sum == GridConstants.MatchTarget)
            {
                _selected = null;
                IsLocked = true;
                _pendingMatchA = previous;
                _pendingMatchB = pos;

                _comboCount = (nowMs - _lastMatchAtMs < GridConstants.ComboWindowMs) ? _comboCount + 1 : 1;
                _lastMatchAtMs = nowMs;

                int gained = GridConstants.BaseMatchScore + (_comboCount - 1) * GridConstants.ComboBonusPerStep;
                Score += gained;

                return TapResult.CreateMatched(previous, pos, gained, _comboCount);
            }

            // 오답: 선택을 완전히 해제한다. (최초 프로토타입은 방금 탭한 타일이 바로
            // 다음 선택으로 이어지는 체이닝이었지만, 실제 플레이해보니 의도치 않게
            // 선택된 채로 남아 다음 시도를 방해한다는 피드백에 따라 2026-08-14 변경 —
            // docs/design/Decisions.md 참고.)
            _selected = null;
            return TapResult.CreateMismatched(previous, pos);
        }

        /// <summary>제거 애니메이션이 끝난 뒤 UI가 호출한다. Tap()과 분리된 이유는 클래스 주석 참고.</summary>
        public MatchResolution ResolvePendingMatch()
        {
            if (!_pendingMatchA.HasValue || !_pendingMatchB.HasValue)
            {
                IsLocked = false;
                return MatchResolution.NoOp;
            }

            var a = _pendingMatchA.Value;
            var b = _pendingMatchB.Value;
            _pendingMatchA = null;
            _pendingMatchB = null;

            Grid[a] = _random.NextTileValue();
            Grid[b] = _random.NextTileValue();

            var resolution = MatchResolution.TilesReplaced;
            if (!Grid.HasValidPair())
            {
                Grid.FillRandom(_random);
                EnsurePlayable();
                resolution = MatchResolution.FullReshuffle;
            }

            IsLocked = false;
            return resolution;
        }

        /// <param name="deltaSeconds">마지막 호출 이후 흐른 시간(초). Unity라면 Time.deltaTime을 그대로 넘기면 된다.</param>
        public void Tick(float deltaSeconds)
        {
            if (IsGameOver)
                return;

            TimeRemainingSeconds -= deltaSeconds;
            if (TimeRemainingSeconds <= 0f)
            {
                TimeRemainingSeconds = 0f;
                IsGameOver = true;
            }
        }

        private void EnsurePlayable()
        {
            int guard = 0;
            while (!Grid.HasValidPair() && guard < GridConstants.MaxReshuffleAttempts)
            {
                Grid.FillRandom(_random);
                guard++;
            }
        }
    }
}
