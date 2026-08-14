using UnityEngine;

namespace TenRush.Managers
{
    /// <summary>
    /// 로컬 최고 점수 저장. 자체 서버가 없는 게임이라 PlayerPrefs 하나로 충분하다
    /// (docs/design/개인정보처리방침_재사용_가이드.md 원칙: 서버 없음 = 방침도 짧아짐).
    /// </summary>
    public static class HighScoreStore
    {
        private const string Key = "TenRush.HighScore";

        public static int Load() => PlayerPrefs.GetInt(Key, 0);

        /// <summary>새 점수가 기존 최고점보다 높을 때만 저장한다. 저장 후 최고점을 반환.</summary>
        public static int SaveIfHigher(int score)
        {
            int best = Load();
            if (score <= best)
                return best;

            PlayerPrefs.SetInt(Key, score);
            PlayerPrefs.Save();
            return score;
        }
    }
}
