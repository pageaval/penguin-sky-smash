using UnityEngine;

namespace PSS
{
    /// Single entry point: builds the entire game from code so no manual
    /// scene wiring is needed. Place one of these in the boot scene.
    public class GameBootstrap : MonoBehaviour
    {
        static bool built;

        // Safety net: build the game on Play even if this component isn't in the scene.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoBoot()
        {
            if (built) return;
            var go = new GameObject("GameBootstrap(Auto)");
            go.AddComponent<GameBootstrap>();
        }

        void Awake()
        {
            if (built) { Destroy(gameObject); return; }
            built = true;
            AssetDB.Init();
            Build();
        }

        void Build()
        {
            // ---- Camera (reuse any existing one so no duplicate camera/listener) ----
            Camera cam = Camera.main;
            GameObject camGo;
            if (cam != null) camGo = cam.gameObject;
            else { camGo = new GameObject("Main Camera"); camGo.tag = "MainCamera"; cam = camGo.AddComponent<Camera>(); }
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.42f, 0.72f, 0.98f);
            cam.nearClipPlane = -20f; cam.farClipPlane = 40f;
            if (camGo.GetComponent<AudioListener>() == null && Object.FindFirstObjectByType<AudioListener>() == null)
                camGo.AddComponent<AudioListener>();
            var follow = camGo.GetComponent<CameraFollow>() ?? camGo.AddComponent<CameraFollow>();
            follow.startPos = new Vector3(-1.3f, 0.5f, -10f);
            camGo.transform.position = follow.startPos;

            // ---- Ground physics ----
            var ground = new GameObject("Ground");
            ground.transform.position = new Vector3(0f, GameConfig.GroundY - 5f, 0f);
            var gcol = ground.AddComponent<BoxCollider2D>();
            gcol.size = new Vector2(200000f, 10f);

            // ---- Bear ----
            var bearGo = new GameObject("Bear");
            var bearSr = bearGo.AddComponent<SpriteRenderer>();
            bearSr.sprite = AssetDB.S("bear_idle");
            bearSr.sortingOrder = 6;
            bearGo.transform.localScale = Vector3.one * 1.7f;
            float bearH = (bearSr.sprite != null ? bearSr.sprite.bounds.size.y : 1.4f) * 1.7f;
            bearGo.transform.position = new Vector3(-6.7f, GameConfig.GroundY + bearH * 0.5f - 0.15f, 0f);
            var bear = bearGo.AddComponent<BearAnimator>();

            // ---- Penguin ----
            var pengGo = new GameObject("Penguin");
            var pengSr = pengGo.AddComponent<SpriteRenderer>();
            pengSr.sprite = AssetDB.S("penguin_idle");
            pengSr.sortingOrder = 12;
            pengGo.transform.localScale = Vector3.one * 1.25f;
            var rb = pengGo.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            var pcol = pengGo.AddComponent<CircleCollider2D>();
            pcol.radius = 0.34f;
            var penguin = pengGo.AddComponent<PenguinController>();
            penguin.StartPos = new Vector3(-5.4f, GameConfig.GroundY + 1.7f, 0f);

            // ---- World ----
            var worldGo = new GameObject("World");
            var world = worldGo.AddComponent<WorldSpawner>();
            world.Build(camGo.transform, pengGo.transform);

            // ---- Managers ----
            var gmGo = new GameObject("GameManager");
            var launch = gmGo.AddComponent<LaunchController>();
            var gm = gmGo.AddComponent<GameManager>();   // Awake loads save + AssetDB
            gm.Launch = launch;
            gm.Penguin = penguin;
            gm.Bear = bear;
            gm.World = world;
            gm.Cam = follow;
            follow.FollowTarget(pengGo.transform);

            // ---- UI ----
            var uiGo = new GameObject("UI");
            var ui = uiGo.AddComponent<UIManager>();
            ui.Init(gm);

            gm.GoToMenu();
        }
    }
}
