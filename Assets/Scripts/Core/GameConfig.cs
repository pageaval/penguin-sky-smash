using UnityEngine;

namespace PSS
{
    /// Central tuning for Penguin Sky Smash. Values adapt the scenario's
    /// arcade formulas to Unity units (PPU 256, orthographic).
    public static class GameConfig
    {
        // ---- Launch (scenario §4) ----
        public const float BaseLaunchSpeed = 17f;   // units/sec at normal hit, bat level 1
        public const float PerfectMult     = 1.35f;
        public const float NormalMult      = 1.00f;
        public const float BadMult         = 0.72f;
        public const float MinAngle        = 20f;   // degrees
        public const float MaxAngle        = 42f;
        public const float PowerBarSpeed   = 1.7f;   // oscillations feel; cycles/sec-ish
        public const float PerfectBand     = 0.10f;  // |t-target|<band => perfect
        public const float GoodBand        = 0.28f;  // else good, else bad

        // ---- Flight physics (scenario §5) ----
        public const float Gravity         = 30f;    // units/sec^2 (downward)
        public const float AirDragX        = 0.28f;  // Rigidbody2D linear damping baseline
        public const float BounceMin       = 2.5f;   // min speed to keep bouncing (units/s)
        public const float MaxSpeed        = 46f;
        public const float StopSpeed       = 1.4f;   // below this on ground => run ends
        public const float GroundY         = -4.3f;

        // ---- Scoring / economy (scenario §9, §11) ----
        public const float MetersPerUnit   = 1.0f;
        public const float ScoreHeightW    = 0.4f;
        public const int   ScoreCoinW      = 10;
        public const int   ScoreComboW     = 150;
        public const int   FishCoinValue   = 1;

        public static int CoinRewardForRun(float distanceMeters, int collectedCoins)
            => Mathf.RoundToInt(distanceMeters / 12f) + collectedCoins;

        // ---- Upgrades (scenario §7) ----
        public const int UpgradeCount = 5;
        public enum Upgrade { BatPower = 0, Glide = 1, Bounce = 2, Magnet = 3, Rocket = 4 }

        public static readonly string[] UpgradeNames =
            { "Bat Power", "Penguin Glide", "Bounce Power", "Coin Magnet", "Rocket Duration" };
        public static readonly string[] UpgradeDesc =
        {
            "Harder swing = faster launch",
            "Less drag, longer flight",
            "Bigger bounce off boosters",
            "Collect coins from farther",
            "Longer rocket boost time",
        };
        public const int MaxUpgradeLevel = 10;

        // cost(level) = 100 * level^1.55  (scenario §11)
        public static int UpgradeCost(int currentLevel)
            => Mathf.RoundToInt(100f * Mathf.Pow(currentLevel, 1.55f));

        // ---- Upgrade effect curves ----
        public static float BatMultiplier(int level)   => 1f + 0.08f * (level - 1);        // §7
        public static float GlideDrag(int level)       => Mathf.Max(0.06f, AirDragX - 0.022f * (level - 1));
        public static float BounceRestitution(int lvl) => Mathf.Min(0.92f, 0.55f + 0.04f * (lvl - 1));
        public static float MagnetRadius(int level)    => 1.1f + 0.35f * (level - 1);
        public static float RocketDuration(int level)  => 1.6f + 0.3f * (level - 1);

        // ---- Biomes (scenario §8). Unlock by best distance. ----
        public struct Biome { public string name; public string bg; public float unlockDist; public float speedTint; }
        public static readonly Biome[] Biomes =
        {
            new Biome{ name="Arctic Day",      bg="Art/Environment/bg_arctic",           unlockDist=0,    speedTint=1.00f },
            new Biome{ name="Sunset Forest",   bg="Art/Environment/bg_forest",           unlockDist=250,  speedTint=1.08f },
            new Biome{ name="Northern Lights", bg="Art/Environment/bg_northern_lights",  unlockDist=600,  speedTint=1.15f },
            new Biome{ name="Golden Woods",    bg="Art/Environment/bg_autumn",           unlockDist=1000, speedTint=1.22f },
        };

        public static int BiomeIndexForDistance(float meters)
        {
            int idx = 0;
            for (int i = 0; i < Biomes.Length; i++)
                if (meters >= Biomes[i].unlockDist) idx = i;
            return idx;
        }
    }
}
