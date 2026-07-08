using UnityEngine;

namespace PSS
{
    public enum ItemType { Coin, BigFish, SealBumper, SpringPad, Rocket, Trampoline, IceWall, Moose, Spike }

    /// A single spawned world object: coin, booster or obstacle.
    public class WorldItem : MonoBehaviour
    {
        public ItemType type;
        bool used;
        SpriteRenderer sr;

        public static WorldItem Create(ItemType type, Vector3 pos, Transform parent)
        {
            var go = new GameObject(type.ToString());
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            var it = go.AddComponent<WorldItem>();
            it.Setup(type);
            return it;
        }

        void Setup(ItemType t)
        {
            type = t;
            sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDB.S(SpriteName(t));
            sr.sortingOrder = 8;

            float worldSize = SizeFor(t);
            if (sr.sprite != null)
            {
                float h = sr.sprite.rect.height / sr.sprite.pixelsPerUnit;
                float s = worldSize / h;
                transform.localScale = new Vector3(s, s, 1f);
            }

            var col = gameObject.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = (sr.sprite != null ? sr.sprite.rect.width * 0.5f / sr.sprite.pixelsPerUnit : 0.5f);
        }

        static string SpriteName(ItemType t) => t switch
        {
            ItemType.Coin       => "coin_fish",
            ItemType.BigFish    => "fish_gold",
            ItemType.SealBumper => "seal_bumper",
            ItemType.SpringPad  => "spring_pad",
            ItemType.Rocket     => "rocket_boost",
            ItemType.Trampoline => "mushroom_bounce",
            ItemType.IceWall    => "ice_wall",
            ItemType.Moose      => "moose_obstacle",
            ItemType.Spike      => "spike_ball",
            _                   => "coin_fish",
        };

        static float SizeFor(ItemType t) => t switch
        {
            ItemType.Coin       => 0.7f,
            ItemType.BigFish    => 0.9f,
            ItemType.SealBumper => 1.5f,
            ItemType.SpringPad  => 1.1f,
            ItemType.Rocket     => 1.0f,
            ItemType.Trampoline => 1.2f,
            ItemType.IceWall    => 1.7f,
            ItemType.Moose      => 1.7f,
            ItemType.Spike      => 1.1f,
            _                   => 0.8f,
        };

        void Update()
        {
            if (type != ItemType.Coin && type != ItemType.BigFish) return;
            var gm = GameManager.Instance;
            if (gm == null || gm.Penguin == null || !gm.Penguin.IsFlying) return;

            // coin magnet
            float radius = GameConfig.MagnetRadius(gm.UpgradeLevel(GameConfig.Upgrade.Magnet));
            Vector3 pp = gm.Penguin.transform.position;
            float d = Vector2.Distance(pp, transform.position);
            if (d < radius)
                transform.position = Vector3.MoveTowards(transform.position, pp, (radius - d + 2f) * Time.deltaTime * 4f);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (used) return;
            var p = other.GetComponent<PenguinController>();
            if (p == null) return;
            used = true;

            switch (type)
            {
                case ItemType.Coin:       p.CollectCoin(GameConfig.FishCoinValue); Destroy(gameObject); break;
                case ItemType.BigFish:    p.CollectCoin(5); Destroy(gameObject); break;
                case ItemType.SealBumper: p.BounceBoost(11f, 4f); Retire(); break;
                case ItemType.SpringPad:  p.BounceBoost(14f, 1.5f); Retire(); break;
                case ItemType.Trampoline: p.BounceBoost(16f, 0f); Retire(); break;
                case ItemType.Rocket:     p.RocketBoost(16f, GameConfig.RocketDuration(GameManager.Instance.UpgradeLevel(GameConfig.Upgrade.Rocket))); Destroy(gameObject); break;
                case ItemType.IceWall:    p.SlowDown(0.55f); Retire(); break;
                case ItemType.Moose:      p.SlowDown(0.7f); Retire(); break;
                case ItemType.Spike:      p.SlowDown(0.4f); Retire(); break;
            }
        }

        // boosters/obstacles stay visible but stop re-triggering
        void Retire()
        {
            var col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }
    }
}
