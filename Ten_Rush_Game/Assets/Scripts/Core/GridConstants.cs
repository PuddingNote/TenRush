namespace TenRush.Core
{
    /// <summary>
    /// 기획서(11장) 확정 상수. 값을 바꿀 땐 이 파일만 수정하면 된다.
    /// </summary>
    public static class GridConstants
    {
        public const int Rows = 7;
        public const int Cols = 6;

        public const int MinTileValue = 1;
        public const int MaxTileValue = 9;
        public const int MatchTarget = 10;

        public const float RoundTimeSeconds = 60f;
        public const float TimerWarningThresholdSeconds = 10f;

        /// <summary>연속 매치로 인정되는 시간 창(ms). 프로토타입의 1500ms 그대로.</summary>
        public const double ComboWindowMs = 1500d;

        public const int BaseMatchScore = 10;
        public const int ComboBonusPerStep = 5;

        /// <summary>전체 리셔플 후에도 유효 페어가 없을 때의 무한루프 방지 가드.</summary>
        public const int MaxReshuffleAttempts = 50;
    }
}
