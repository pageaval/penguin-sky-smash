using System;
using UnityEngine;

namespace PSS
{
    public enum GameState { Menu, Aim, Flying, Result, Upgrade }

    public struct RunResult
    {
        public float distance;      // meters
        public float maxHeight;     // meters
        public int   coins;
        public int   maxCombo;
        public int   score;
        public bool  newRecord;
        public int   coinReward;
    }

    /// Central hub + state machine. Created by GameBootstrap. Access via Instance.
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public SaveData Data;
        public GameState State { get; private set; }

        // subsystem refs (assigned by GameBootstrap)
        [NonSerialized] public LaunchController Launch;
        [NonSerialized] public PenguinController Penguin;
        [NonSerialized] public BearAnimator Bear;
        [NonSerialized] public WorldSpawner World;
        [NonSerialized] public CameraFollow Cam;

        // live run stats
        public float Distance { get; private set; }
        public float MaxHeight { get; private set; }
        public int   Coins { get; private set; }
        public int   Combo { get; private set; }
        public int   MaxCombo { get; private set; }
        public bool  PerfectLaunch { get; private set; }

        public event Action<GameState> OnStateChanged;
        public event Action OnStatsChanged;
        public event Action<RunResult> OnRunFinished;
        public event Action OnDataChanged;

        void Awake()
        {
            Instance = this;
            AssetDB.Init();
            Data = SaveSystem.Load();
        }

        public void RaiseDataChanged() => OnDataChanged?.Invoke();

        void SetState(GameState s)
        {
            State = s;
            OnStateChanged?.Invoke(s);
        }

        // ---- Flow ----
        public void GoToMenu()
        {
            SetState(GameState.Menu);
            Cam?.SnapToStart();
            Penguin?.ResetToStart();
            Bear?.SetPose(BearPose.Idle);
            World?.ResetWorld();
        }

        public void BeginAim()
        {
            Distance = MaxHeight = 0;
            Coins = Combo = MaxCombo = 0;
            PerfectLaunch = false;
            Penguin.ResetToStart();
            Bear.SetPose(BearPose.Aim);
            World.ResetWorld();
            Cam.SnapToStart();
            SetState(GameState.Aim);
            Launch.BeginAiming();
            OnStatsChanged?.Invoke();
        }

        /// Called by LaunchController when the player taps to hit.
        public void ConfirmLaunch(float speed, float angleDeg, HitQuality quality)
        {
            PerfectLaunch = quality == HitQuality.Perfect;
            Bear.Swing(() =>
            {
                Penguin.Launch(speed, angleDeg);
                Cam?.FollowTarget(Penguin.transform);
                SetState(GameState.Flying);
            });
        }

        // ---- Live updates from PenguinController ----
        public void ReportFlight(float distanceMeters, float heightMeters)
        {
            if (State != GameState.Flying) return;
            Distance = Mathf.Max(Distance, distanceMeters);
            MaxHeight = Mathf.Max(MaxHeight, heightMeters);
            OnStatsChanged?.Invoke();
        }

        public void AddCoins(int v)
        {
            Coins += v;
            OnStatsChanged?.Invoke();
        }

        public void RegisterBoosterHit()
        {
            Combo++;
            MaxCombo = Mathf.Max(MaxCombo, Combo);
            OnStatsChanged?.Invoke();
        }

        public void BreakCombo() { Combo = 0; }

        public int CurrentBiome => GameConfig.BiomeIndexForDistance(Distance);

        // ---- End of run ----
        public void EndRun()
        {
            if (State != GameState.Flying) return;

            int score = Mathf.RoundToInt(
                Distance * 1f +
                MaxHeight * GameConfig.ScoreHeightW +
                Coins * GameConfig.ScoreCoinW +
                MaxCombo * GameConfig.ScoreComboW +
                (PerfectLaunch ? 200 : 0));

            bool newRecord = Distance > Data.bestDistance;
            if (newRecord) Data.bestDistance = Distance;
            if (score > Data.bestScore) Data.bestScore = score;

            int reward = GameConfig.CoinRewardForRun(Distance, Coins);
            Data.coins += reward;
            Data.runsPlayed++;
            SaveSystem.Save(Data);

            var result = new RunResult
            {
                distance = Distance, maxHeight = MaxHeight, coins = Coins,
                maxCombo = MaxCombo, score = score, newRecord = newRecord, coinReward = reward
            };

            Bear.SetPose(BearPose.Victory);
            SetState(GameState.Result);
            OnRunFinished?.Invoke(result);
            OnDataChanged?.Invoke();
        }

        public void OpenUpgrades() => SetState(GameState.Upgrade);

        // ---- Economy ----
        public bool TryBuyUpgrade(GameConfig.Upgrade u)
        {
            int idx = (int)u;
            int level = Data.upgradeLevels[idx];
            if (level >= GameConfig.MaxUpgradeLevel) return false;
            int cost = GameConfig.UpgradeCost(level);
            if (Data.coins < cost) return false;
            Data.coins -= cost;
            Data.upgradeLevels[idx] = level + 1;
            SaveSystem.Save(Data);
            OnDataChanged?.Invoke();
            return true;
        }

        public int UpgradeLevel(GameConfig.Upgrade u) => Data.upgradeLevels[(int)u];
    }
}
