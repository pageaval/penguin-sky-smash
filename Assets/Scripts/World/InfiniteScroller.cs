using UnityEngine;

namespace PSS
{
    /// Tiled, recycling horizontal layer with a parallax factor.
    /// factor = 1 -> world-locked (ground); factor < 1 -> distant/slow (sky, mountains).
    public class InfiniteScroller : MonoBehaviour
    {
        Transform cam;
        float factor;
        float tileW;
        int count;
        Transform[] tiles;

        public void Build(Sprite sprite, int count, float y, float worldHeight, int order, float factor, Transform cam)
        {
            this.cam = cam;
            this.factor = factor;
            this.count = count;
            tiles = new Transform[count];

            float scale = worldHeight / (sprite.rect.height / sprite.pixelsPerUnit);
            tileW = (sprite.rect.width / sprite.pixelsPerUnit) * scale;

            for (int i = 0; i < count; i++)
            {
                var go = new GameObject("tile" + i);
                go.transform.SetParent(transform, false);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.sortingOrder = order;
                go.transform.localScale = new Vector3(scale, scale, 1f);
                go.transform.localPosition = new Vector3((i - count / 2) * tileW, 0f, 0f);
                tiles[i] = go.transform;
            }
            transform.position = new Vector3(0f, y, 0f);
            Reposition();
        }

        public void SetSprite(Sprite sprite)
        {
            if (tiles == null) return;
            foreach (var t in tiles)
            {
                var sr = t.GetComponent<SpriteRenderer>();
                if (sr != null) sr.sprite = sprite;
            }
        }

        public void Reposition()
        {
            if (cam == null) return;
            float rootX = cam.position.x * (1f - factor);
            transform.position = new Vector3(rootX, transform.position.y, 0f);
            float camX = cam.position.x;
            float span = tileW * count;
            foreach (var t in tiles)
            {
                float worldX = rootX + t.localPosition.x;
                while (worldX + tileW * 0.5f < camX - span * 0.5f) { t.localPosition += Vector3.right * span; worldX += span; }
                while (worldX - tileW * 0.5f > camX + span * 0.5f) { t.localPosition -= Vector3.right * span; worldX -= span; }
            }
        }

        void LateUpdate() => Reposition();
    }
}
