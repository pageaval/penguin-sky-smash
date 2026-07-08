using UnityEngine;

namespace PSS
{
    [System.Serializable]
    public class SaveData
    {
        public int   coins            = 0;
        public float bestDistance     = 0f;
        public int   bestScore        = 0;
        // upgrade levels (1-based), index by GameConfig.Upgrade
        public int[] upgradeLevels    = { 1, 1, 1, 1, 1 };
        public int   selectedBat      = 0;   // index into bat sprite list
        public int   runsPlayed       = 0;
        public bool  soundOn          = true;
        public string lastDailyClaim  = "";  // yyyyMMdd
    }

    /// JSON-in-PlayerPrefs persistence. Single source of truth: GameManager.Data.
    public static class SaveSystem
    {
        const string Key = "pss_save_v1";

        public static SaveData Load()
        {
            if (PlayerPrefs.HasKey(Key))
            {
                try
                {
                    var d = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(Key));
                    if (d != null)
                    {
                        if (d.upgradeLevels == null || d.upgradeLevels.Length != GameConfig.UpgradeCount)
                            d.upgradeLevels = new[] { 1, 1, 1, 1, 1 };
                        return d;
                    }
                }
                catch { /* corrupt -> fresh */ }
            }
            return new SaveData();
        }

        public static void Save(SaveData d)
        {
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(d));
            PlayerPrefs.Save();
        }

        public static void Wipe()
        {
            PlayerPrefs.DeleteKey(Key);
            PlayerPrefs.Save();
        }
    }
}
