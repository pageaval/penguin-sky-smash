using System.Collections.Generic;
using UnityEngine;

namespace PSS
{
    /// Builds the scrolling world (background + ground) and spawns coins,
    /// boosters and obstacles ahead of the penguin based on distance.
    public class WorldSpawner : MonoBehaviour
    {
        Transform cam, penguin;
        InfiniteScroller bg, ground;
        Transform itemRoot;
        readonly List<WorldItem> items = new();

        float nextSpawnX;
        int biome = -1;
        const float LookAhead = 22f;
        const float FirstSpawnX = 3f;

        public void Build(Transform cam, Transform penguin)
        {
            this.cam = cam;
            this.penguin = penguin;

            var bgGo = new GameObject("BackgroundLayer");
            bgGo.transform.SetParent(transform, false);
            bg = bgGo.AddComponent<InfiniteScroller>();
            bg.Build(AssetDB.S("bg_arctic"), 3, 0.6f, 12.5f, -30, 0.5f, cam);

            var grGo = new GameObject("GroundLayer");
            grGo.transform.SetParent(transform, false);
            ground = grGo.AddComponent<InfiniteScroller>();
            ground.Build(AssetDB.S("ground_ice"), 4, GameConfig.GroundY - 0.35f, 1.9f, -8, 1f, cam);

            itemRoot = new GameObject("Items").transform;
            itemRoot.SetParent(transform, false);

            SetBiome(0, force: true);
        }

        public void ResetWorld()
        {
            for (int i = items.Count - 1; i >= 0; i--)
                if (items[i] != null) Destroy(items[i].gameObject);
            items.Clear();
            nextSpawnX = penguin.position.x + FirstSpawnX;
            SetBiome(0, force: true);
            bg?.Reposition();
            ground?.Reposition();
        }

        void SetBiome(int idx, bool force = false)
        {
            if (idx == biome && !force) return;
            biome = idx;
            var b = GameConfig.Biomes[Mathf.Clamp(idx, 0, GameConfig.Biomes.Length - 1)];
            var sp = AssetDB.S(b.bg);
            if (sp != null) bg.SetSprite(sp);
        }

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.State != GameState.Flying) return;

            SetBiome(gm.CurrentBiome);

            while (penguin.position.x + LookAhead > nextSpawnX)
                SpawnCluster(gm.Distance);

            // cleanup behind camera
            float cut = cam.position.x - 16f;
            for (int i = items.Count - 1; i >= 0; i--)
            {
                if (items[i] == null) { items.RemoveAt(i); continue; }
                if (items[i].transform.position.x < cut) { Destroy(items[i].gameObject); items.RemoveAt(i); }
            }
        }

        void SpawnCluster(float distance)
        {
            float x = nextSpawnX;
            float t = Mathf.Clamp01(distance / 800f);        // difficulty ramp

            // coin arc
            int n = Random.Range(3, 6);
            float baseY = GameConfig.GroundY + Random.Range(1.2f, 3.2f);
            float arcH = Random.Range(1.0f, 2.6f);
            for (int i = 0; i < n; i++)
            {
                float cx = x + i * 0.85f;
                float cy = baseY + Mathf.Sin((i + 0.5f) / n * Mathf.PI) * arcH;
                Add(ItemType.Coin, new Vector3(cx, cy, 0));
            }

            // sometimes a big fish
            if (Random.value < 0.18f)
                Add(ItemType.BigFish, new Vector3(x + Random.Range(0f, 3f), baseY + arcH + 1f, 0));

            // booster or obstacle
            float roll = Random.value;
            float boosterChance = 0.45f;
            float obstacleChance = 0.20f + 0.30f * t;
            Vector3 groundPos = new Vector3(x + Random.Range(2f, 4f), GameConfig.GroundY + 0.7f, 0);

            if (roll < boosterChance)
            {
                ItemType[] boosters = { ItemType.SealBumper, ItemType.SpringPad, ItemType.Rocket, ItemType.Trampoline };
                var bt = boosters[Random.Range(0, boosters.Length)];
                Vector3 p = bt == ItemType.Rocket ? new Vector3(x + 2f, GameConfig.GroundY + Random.Range(1.5f, 3.5f), 0) : groundPos;
                Add(bt, p);
            }
            else if (roll < boosterChance + obstacleChance)
            {
                ItemType[] obstacles = { ItemType.IceWall, ItemType.Moose, ItemType.Spike };
                Add(obstacles[Random.Range(0, obstacles.Length)], groundPos);
            }

            nextSpawnX += Mathf.Lerp(9f, 6.5f, t) + Random.Range(0f, 2.5f);
        }

        void Add(ItemType type, Vector3 pos)
        {
            items.Add(WorldItem.Create(type, pos, itemRoot));
        }
    }
}
