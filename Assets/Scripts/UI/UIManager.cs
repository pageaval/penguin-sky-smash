using UnityEngine;
using UnityEngine.UI;

namespace PSS
{
    /// Builds and drives every screen from code: Menu, HUD, Result, Upgrade.
    /// Dynamic numbers use clean generated rounded backgrounds (UITex) instead of
    /// art sprites that have placeholder numbers baked in.
    public class UIManager : MonoBehaviour
    {
        GameManager gm;

        RectTransform menu, hud, result, upgrade;

        // palette
        static readonly Color Cream   = new Color(0.99f, 0.96f, 0.86f, 1f);
        static readonly Color PanelBg = new Color(0.10f, 0.22f, 0.40f, 0.98f);
        static readonly Color PillNavy= new Color(0.09f, 0.19f, 0.36f, 0.92f);
        static readonly Color TextDark= new Color(0.13f, 0.20f, 0.34f, 1f);

        // HUD
        Text distanceLabel, coinsLabel, comboLabel;
        RectTransform comboRoot, powerRoot, powerFill;
        Image powerFillImg;
        Text tapLabel;
        const float powerW = 720f;

        // Menu
        Text menuBest, menuCoins;

        // Result
        Text rDist, rHeight, rCoins, rScore, rReward;
        RectTransform rNewRecord;
        Image[] rStars = new Image[3];

        // Upgrade
        Text upCoins;
        Text[] upLevel = new Text[GameConfig.UpgradeCount];
        Text[] upCost  = new Text[GameConfig.UpgradeCount];

        public void Init(GameManager g)
        {
            gm = g;
            UIFactory.EnsureEventSystem();
            var canvas = UIFactory.CreateCanvas("UICanvas", 100);
            BuildMenu(canvas.transform);
            BuildHud(canvas.transform);
            BuildResult(canvas.transform);
            BuildUpgrade(canvas.transform);

            gm.OnStateChanged += ShowState;
            gm.OnStatsChanged += RefreshHud;
            gm.OnRunFinished  += FillResult;
            gm.OnDataChanged  += RefreshAll;

            ShowState(GameState.Menu);
            RefreshAll();
        }

        // ---- small helpers ----
        // A rounded pill with an icon on the left and a right-aligned number. Returns the number Text.
        Text IconPill(Transform parent, string iconSprite, Vector2 aMin, Vector2 aMax, Vector2 pivot,
                      Vector2 pos, Vector2 size, int textSize)
        {
            var box = UIFactory.RoundedBox(parent, "pill", PillNavy, 26, aMin, aMax, pivot, pos, size,
                new Color(1, 1, 1, 0.15f), 3);
            float h = size.y;
            UIFactory.SpriteImage(box.transform, iconSprite,
                new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                new Vector2(h * 0.5f + 6, 0), new Vector2(h * 0.82f, h * 0.82f));
            return UIFactory.Label(box.transform, "0", textSize, UIFactory.White, TextAnchor.MiddleRight,
                new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f),
                new Vector2(-24, 0), new Vector2(size.x - h - 20, size.y));
        }

        // ---------------- MENU ----------------
        void BuildMenu(Transform parent)
        {
            menu = UIFactory.FullScreen(parent, "MenuScreen");

            UIFactory.SpriteImage(menu, "ribbon_banner",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0, -60), new Vector2(760, 200));
            UIFactory.Label(menu, "PENGUIN SKY SMASH", 52, UIFactory.Navy, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0, -112), new Vector2(700, 90));

            // best distance pill (center)
            var bestBox = UIFactory.RoundedBox(menu, "best", PillNavy, 30,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 210), new Vector2(520, 82), new Color(1, 1, 1, 0.15f), 3);
            menuBest = UIFactory.Label(bestBox.transform, "BEST  0 M", 42, UIFactory.Gold, TextAnchor.MiddleCenter,
                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);

            // coins pill top-right
            menuCoins = IconPill(menu, "coin_fish",
                new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1),
                new Vector2(-30, -34), new Vector2(240, 78), 40);

            UIFactory.Button(menu, "btn_blue_wide", "PLAY", 60,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 20), new Vector2(540, 158), () => gm.BeginAim());

            UIFactory.Button(menu, "btn_blue_long", "UPGRADE", 42,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -150), new Vector2(420, 120), () => gm.OpenUpgrades());
        }

        // ---------------- HUD ----------------
        void BuildHud(Transform parent)
        {
            hud = UIFactory.FullScreen(parent, "HudScreen");

            UIFactory.Button(hud, "btn_pause", "", 0,
                new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1),
                new Vector2(28, -28), new Vector2(96, 92), () => gm.GoToMenu());

            // distance pill top-center (flag icon + number)
            distanceLabel = IconPill(hud, "flag_marker",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0, -30), new Vector2(320, 84), 46);

            // coins pill top-right
            coinsLabel = IconPill(hud, "coin_fish",
                new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1),
                new Vector2(-30, -30), new Vector2(230, 84), 42);

            // combo (center-upper)
            comboRoot = UIFactory.Rect(hud, "Combo");
            UIFactory.SetAnchors(comboRoot, new Vector2(0.5f, 0.74f), new Vector2(0.5f, 0.74f),
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(420, 160));
            UIFactory.SpriteImage(comboRoot, "fx_star_burst",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(300, 300));
            comboLabel = UIFactory.Label(comboRoot, "COMBO x2", 48, UIFactory.White, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(420, 80));
            comboRoot.gameObject.SetActive(false);

            // power bar (bottom center, aim only)
            powerRoot = UIFactory.Rect(hud, "PowerBar");
            UIFactory.SetAnchors(powerRoot, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0.5f, 0f), new Vector2(0, 70), new Vector2(powerW, 54));
            var barBg = powerRoot.gameObject.AddComponent<Image>();
            barBg.sprite = UITex.Rounded(24, new Color(0.05f, 0.12f, 0.22f, 0.95f), new Color(1,1,1,0.2f), 3);
            barBg.type = Image.Type.Sliced;
            // perfect zone marker (0.80..0.98)
            var zone = UIFactory.Rect(powerRoot, "PerfectZone");
            UIFactory.SetAnchors(zone, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                new Vector2(0.80f * powerW, 0), new Vector2(0.18f * powerW, 46));
            var zoneImg = zone.gameObject.AddComponent<Image>();
            zoneImg.sprite = UITex.Rounded(10, new Color(0.3f, 1f, 0.45f, 0.4f)); zoneImg.type = Image.Type.Sliced;
            // fill (left-anchored, dynamic width)
            powerFill = UIFactory.Rect(powerRoot, "Fill");
            UIFactory.SetAnchors(powerFill, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                new Vector2(6, 0), new Vector2(0, 40));
            powerFillImg = powerFill.gameObject.AddComponent<Image>();
            powerFillImg.sprite = UITex.Rounded(18, Color.white); powerFillImg.type = Image.Type.Sliced;
            powerFillImg.color = UIFactory.Gold;

            tapLabel = UIFactory.Label(hud, "TAP  TO  HIT!", 48, UIFactory.Gold, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0, 150), new Vector2(620, 70));
        }

        // ---------------- RESULT ----------------
        void BuildResult(Transform parent)
        {
            result = UIFactory.FullScreen(parent, "ResultScreen");
            var dim = result.gameObject.AddComponent<Image>();
            dim.color = new Color(0, 0, 0, 0.5f);

            var panel = UIFactory.RoundedBox(result, "panel", Cream, 40,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 0), new Vector2(920, 700), new Color(0.16f, 0.28f, 0.5f, 1f), 8);

            UIFactory.SpriteImage(panel.transform, "ribbon_banner",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0, 44), new Vector2(560, 150));
            UIFactory.Label(panel.transform, "RESULTS", 46, UIFactory.White, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0, 14), new Vector2(520, 70));

            // stars
            for (int i = 0; i < 3; i++)
            {
                rStars[i] = UIFactory.SpriteImage(panel.transform, "star_bonus",
                    new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                    new Vector2((i - 1) * 110, -70), new Vector2(96, 96));
            }

            rNewRecord = (RectTransform)UIFactory.Label(panel.transform, "NEW  RECORD!", 40, new Color(1f, 0.5f, 0.2f),
                TextAnchor.MiddleCenter, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0, -140), new Vector2(560, 56)).rectTransform;

            rDist   = ResultRow(panel.transform, "DISTANCE",   -30);
            rHeight = ResultRow(panel.transform, "MAX HEIGHT", -100);
            rCoins  = ResultRow(panel.transform, "COINS",      -170);
            rScore  = ResultRow(panel.transform, "SCORE",      -240);

            rReward = UIFactory.Label(panel.transform, "+0", 42, new Color(0.85f, 0.55f, 0.05f), TextAnchor.MiddleCenter,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(24, -300), new Vector2(300, 56));
            UIFactory.SpriteImage(panel.transform, "coin_fish",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-70, -300), new Vector2(52, 52));

            UIFactory.Button(result, "btn_blue_wide", "RETRY", 46,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -300), new Vector2(430, 130), () => gm.BeginAim());
            UIFactory.Button(result, "btn_blue_long", "UPGRADE", 34,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-160, -420), new Vector2(290, 104), () => gm.OpenUpgrades());
            UIFactory.Button(result, "btn_blue_long", "MENU", 34,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(160, -420), new Vector2(290, 104), () => gm.GoToMenu());
        }

        Text ResultRow(Transform parent, string name, float y)
        {
            UIFactory.Label(parent, name, 34, TextDark, TextAnchor.MiddleLeft,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-320, y), new Vector2(420, 50));
            return UIFactory.Label(parent, "0", 40, new Color(0.10f, 0.32f, 0.6f), TextAnchor.MiddleRight,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(330, y), new Vector2(360, 54));
        }

        // ---------------- UPGRADE ----------------
        void BuildUpgrade(Transform parent)
        {
            upgrade = UIFactory.FullScreen(parent, "UpgradeScreen");
            var bg = upgrade.gameObject.AddComponent<Image>();
            bg.color = PanelBg;

            UIFactory.Label(upgrade, "UPGRADES", 58, UIFactory.Gold, TextAnchor.MiddleCenter,
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0, -58), new Vector2(600, 80));

            upCoins = IconPill(upgrade, "coin_fish",
                new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1),
                new Vector2(-30, -34), new Vector2(240, 78), 40);

            for (int i = 0; i < GameConfig.UpgradeCount; i++)
            {
                int idx = i;
                float y = 250 - i * 132;
                var row = UIFactory.RoundedBox(upgrade, "row" + i, new Color(1, 1, 1, 0.10f), 22,
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(0, y), new Vector2(1120, 118));

                UIFactory.Label(row.transform, GameConfig.UpgradeNames[i], 38, UIFactory.White, TextAnchor.MiddleLeft,
                    new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                    new Vector2(36, 22), new Vector2(560, 48));
                UIFactory.Label(row.transform, GameConfig.UpgradeDesc[i], 24, new Color(0.75f, 0.85f, 1f), TextAnchor.MiddleLeft,
                    new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f),
                    new Vector2(36, -28), new Vector2(620, 40));
                upLevel[i] = UIFactory.Label(row.transform, "Lv 1", 34, UIFactory.Gold, TextAnchor.MiddleCenter,
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(230, 0), new Vector2(180, 48));

                var buy = UIFactory.Button(row.transform, "btn_blue_long", "", 30,
                    new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f),
                    new Vector2(-24, 0), new Vector2(300, 98), () => Buy(idx));
                UIFactory.SpriteImage(buy.transform, "coin_fish",
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(-70, 4), new Vector2(46, 46));
                upCost[i] = UIFactory.Label(buy.transform, "100", 34, UIFactory.White, TextAnchor.MiddleCenter,
                    new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                    new Vector2(30, 4), new Vector2(180, 50));
            }

            UIFactory.Button(upgrade, "btn_blue_wide", "BACK", 44,
                new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f),
                new Vector2(0, 50), new Vector2(380, 120), () => gm.GoToMenu());
        }

        void Buy(int idx)
        {
            if (gm.TryBuyUpgrade((GameConfig.Upgrade)idx))
                Fx.Spawn("fx_star", gm.Bear != null ? gm.Bear.transform.position : Vector3.zero, 1.2f);
            RefreshUpgrade();
        }

        // ---------------- STATE + REFRESH ----------------
        void ShowState(GameState s)
        {
            menu.gameObject.SetActive(s == GameState.Menu);
            upgrade.gameObject.SetActive(s == GameState.Upgrade);
            result.gameObject.SetActive(s == GameState.Result);
            hud.gameObject.SetActive(s == GameState.Aim || s == GameState.Flying);
            powerRoot.gameObject.SetActive(s == GameState.Aim);
            tapLabel.gameObject.SetActive(s == GameState.Aim);
            if (s == GameState.Menu) RefreshMenu();
            if (s == GameState.Upgrade) RefreshUpgrade();
            if (s == GameState.Aim || s == GameState.Flying) RefreshHud();
        }

        void RefreshAll() { RefreshMenu(); RefreshUpgrade(); RefreshHud(); }

        void RefreshMenu()
        {
            if (menuBest == null) return;
            menuBest.text = $"BEST  {Mathf.RoundToInt(gm.Data.bestDistance)} M";
            menuCoins.text = gm.Data.coins.ToString();
        }

        void RefreshHud()
        {
            if (distanceLabel == null) return;
            distanceLabel.text = $"{Mathf.RoundToInt(gm.Distance)} M";
            coinsLabel.text = gm.Coins.ToString();
            bool showCombo = gm.Combo >= 2;
            comboRoot.gameObject.SetActive(showCombo);
            if (showCombo) comboLabel.text = "COMBO x" + gm.Combo;
        }

        void FillResult(RunResult r)
        {
            rDist.text = $"{Mathf.RoundToInt(r.distance)} M";
            rHeight.text = $"{Mathf.RoundToInt(r.maxHeight)} M";
            rCoins.text = r.coins.ToString();
            rScore.text = r.score.ToString();
            rReward.text = $"+{r.coinReward}";
            rNewRecord.gameObject.SetActive(r.newRecord);
            int stars = r.distance >= 120 ? 3 : r.distance >= 55 ? 2 : r.distance >= 15 ? 1 : 0;
            for (int i = 0; i < 3; i++)
                rStars[i].color = i < stars ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.45f);
        }

        void RefreshUpgrade()
        {
            if (upCoins == null) return;
            upCoins.text = gm.Data.coins.ToString();
            for (int i = 0; i < GameConfig.UpgradeCount; i++)
            {
                int level = gm.Data.upgradeLevels[i];
                upLevel[i].text = "Lv " + level;
                if (level >= GameConfig.MaxUpgradeLevel) upCost[i].text = "MAX";
                else
                {
                    int cost = GameConfig.UpgradeCost(level);
                    upCost[i].text = cost.ToString();
                    upCost[i].color = gm.Data.coins >= cost ? UIFactory.White : new Color(1f, 0.7f, 0.7f);
                }
            }
        }

        // ---------------- power bar per-frame ----------------
        void Update()
        {
            if (gm == null || gm.State != GameState.Aim || gm.Launch == null) return;
            float p = gm.Launch.Power;
            powerFill.sizeDelta = new Vector2((powerW - 12) * p, 40);
            bool perfect = p >= 0.80f && p <= 0.98f;
            powerFillImg.color = perfect ? UIFactory.Green : UIFactory.Gold;
            float blink = Mathf.PingPong(Time.unscaledTime * 2.5f, 1f);
            var c = tapLabel.color; c.a = 0.5f + 0.5f * blink; tapLabel.color = c;
        }
    }
}
