using NUnit.Framework;
using TenRush.Core;
using TenRush.Core.Model;

namespace TenRush.Tests
{
    public class GameRoundTests
    {
        // 3장 자리에 4가 있어 (0,1)+(1,0)=3+7=10, (0,0)+(1,1)=1+9=10 등 여러 유효 페어를 가진 고정 보드.
        // 7x6 = 42칸을 전부 채운다.
        private static readonly int[,] FixedPlayableGrid =
        {
            { 1, 2, 3, 4, 5, 6 },
            { 9, 8, 7, 6, 5, 4 },
            { 1, 2, 3, 4, 5, 6 },
            { 9, 8, 7, 6, 5, 4 },
            { 1, 2, 3, 4, 5, 6 },
            { 9, 8, 7, 6, 5, 4 },
            { 1, 2, 3, 4, 5, 6 },
        };

        private static GameRound NewFixedRound()
        {
            var round = new GameRound(new FakeRandomSource(System.Array.Empty<int>()));
            round.Grid.LoadFrom(FixedPlayableGrid);
            return round;
        }

        [Test]
        public void FirstTap_SelectsCell()
        {
            var round = NewFixedRound();

            var result = round.Tap(0, 0, nowMs: 0);

            Assert.AreEqual(TapOutcome.Selected, result.Outcome);
            Assert.AreEqual(new GridPosition(0, 0), result.Selected);
        }

        [Test]
        public void TappingSameCellTwice_Deselects()
        {
            var round = NewFixedRound();
            round.Tap(0, 0, nowMs: 0);

            var result = round.Tap(0, 0, nowMs: 10);

            Assert.AreEqual(TapOutcome.Deselected, result.Outcome);
        }

        [Test]
        public void TwoCellsSummingToTen_ReturnsMatchedAndLocksBoard()
        {
            var round = NewFixedRound();
            round.Tap(0, 0, nowMs: 0); // value 1

            var result = round.Tap(1, 0, nowMs: 10); // value 9 -> sum 10

            Assert.AreEqual(TapOutcome.Matched, result.Outcome);
            Assert.AreEqual(10, result.ScoreGained);
            Assert.AreEqual(1, result.ComboCount);
            Assert.AreEqual(10, round.Score);
            Assert.IsTrue(round.IsLocked);
        }

        [Test]
        public void TwoCellsNotSummingToTen_ReturnsMismatchAndReselectsSecondTile()
        {
            var round = NewFixedRound();
            round.Tap(0, 0, nowMs: 0); // value 1
            var result = round.Tap(0, 1, nowMs: 10); // value 2 -> sum 3

            Assert.AreEqual(TapOutcome.Mismatched, result.Outcome);
            Assert.AreEqual(0, round.Score);

            // 오답 타일이 바로 다음 선택으로 이어지므로, 그 타일과 합 10인 칸을 탭하면 곧장 매치된다.
            var followUp = round.Tap(1, 1, nowMs: 20); // value 8, 2+8=10
            Assert.AreEqual(TapOutcome.Matched, followUp.Outcome);
        }

        [Test]
        public void TapWhileLocked_IsIgnored()
        {
            var round = NewFixedRound();
            round.Tap(0, 0, nowMs: 0);
            round.Tap(1, 0, nowMs: 10); // matches -> locked

            var result = round.Tap(2, 0, nowMs: 20);

            Assert.AreEqual(TapOutcome.Ignored, result.Outcome);
        }

        [Test]
        public void SecondMatchWithinComboWindow_AddsComboBonus()
        {
            var round = NewFixedRound();
            round.Tap(0, 0, nowMs: 0);
            round.Tap(1, 0, nowMs: 10); // 1st match, +10, combo 1
            round.ResolvePendingMatch();

            round.Tap(0, 1, nowMs: 100); // value 2
            var second = round.Tap(1, 1, nowMs: 200); // value 8 -> sum 10, within 1500ms window

            Assert.AreEqual(TapOutcome.Matched, second.Outcome);
            Assert.AreEqual(2, second.ComboCount);
            Assert.AreEqual(15, second.ScoreGained); // 10 + (2-1)*5
            Assert.AreEqual(25, round.Score);
        }

        [Test]
        public void SecondMatchAfterComboWindowExpires_ResetsToComboOne()
        {
            var round = NewFixedRound();
            round.Tap(0, 0, nowMs: 0);
            round.Tap(1, 0, nowMs: 10); // 1st match at t=10
            round.ResolvePendingMatch();

            round.Tap(0, 1, nowMs: 5000);
            var second = round.Tap(1, 1, nowMs: 5001); // far outside 1500ms window

            Assert.AreEqual(1, second.ComboCount);
            Assert.AreEqual(10, second.ScoreGained);
        }

        [Test]
        public void ResolvePendingMatch_ReplacesTilesWithinValidRange()
        {
            var round = NewFixedRound();
            round.Tap(0, 0, nowMs: 0);
            round.Tap(1, 0, nowMs: 10);

            var resolution = round.ResolvePendingMatch();

            Assert.AreNotEqual(MatchResolution.NoOp, resolution);
            Assert.IsFalse(round.IsLocked);
            Assert.GreaterOrEqual(round.Grid[0, 0], GridConstants.MinTileValue);
            Assert.LessOrEqual(round.Grid[0, 0], GridConstants.MaxTileValue);
            Assert.GreaterOrEqual(round.Grid[1, 0], GridConstants.MinTileValue);
            Assert.LessOrEqual(round.Grid[1, 0], GridConstants.MaxTileValue);
        }

        [Test]
        public void ResolvePendingMatch_WhenNoValidPairRemains_TriggersFullReshuffle()
        {
            // 전부 1로 채운 뒤 (0,0)=1,(0,1)=9 딱 한 쌍만 유효하게 만든다.
            var uniformGrid = new int[7, 6];
            for (int r = 0; r < 7; r++)
                for (int c = 0; c < 6; c++)
                    uniformGrid[r, c] = 1;
            uniformGrid[0, 1] = 9;

            // fallback=5로 두면 construction 중 FillRandom이 곧바로 유효 페어(5+5)를 만들어
            // EnsurePlayable의 50회 재시도 루프를 낭비하지 않는다. queue는 비워 둬서
            // construction이 아무 것도 소비하지 않게 한 뒤, LoadFrom으로 보드를 덮어쓴다.
            var fakeRandom = new FakeRandomSource(System.Array.Empty<int>(), fallback: 5);
            var round = new GameRound(fakeRandom);
            round.Grid.LoadFrom(uniformGrid);

            round.Tap(0, 0, nowMs: 0); // 1
            round.Tap(0, 1, nowMs: 10); // 9 -> matched, 보드의 유일한 유효 페어가 사라짐

            // 매치된 두 칸을 1,1로 채우면(=전부 1) 유효 페어가 없어져 전체 리셔플이 일어나야 하고,
            // 그 리셔플의 첫 두 칸이 1,9라서 리셔플 직후에는 다시 유효 페어가 생긴다.
            fakeRandom.Enqueue(1, 1, 1, 9);

            var resolution = round.ResolvePendingMatch();

            Assert.AreEqual(MatchResolution.FullReshuffle, resolution);
            Assert.IsTrue(round.Grid.HasValidPair());
        }

        [Test]
        public void Tick_EndsGameWhenTimeReachesZero()
        {
            var round = NewFixedRound();

            round.Tick(GridConstants.RoundTimeSeconds);

            Assert.IsTrue(round.IsGameOver);
            Assert.AreEqual(0f, round.TimeRemainingSeconds);
        }

        [Test]
        public void TapAfterGameOver_IsIgnored()
        {
            var round = NewFixedRound();
            round.Tick(GridConstants.RoundTimeSeconds);

            var result = round.Tap(0, 0, nowMs: 0);

            Assert.AreEqual(TapOutcome.Ignored, result.Outcome);
        }
    }
}
