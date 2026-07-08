using UnityEngine;

namespace PSS
{
    /// Fire-and-forget sprite effect (pops, scales up and fades out).
    public static class Fx
    {
        public static void Spawn(string sprite, Vector3 pos, float scale = 1f)
        {
            var s = AssetDB.S(sprite);
            if (s == null) return;
            var go = new GameObject("fx_" + sprite);
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = s;
            sr.sortingOrder = 50;
            go.transform.localScale = Vector3.one * scale * 0.6f;
            go.AddComponent<FxInstance>().Init(scale);
        }
    }

    public class FxInstance : MonoBehaviour
    {
        SpriteRenderer sr;
        float life, maxLife = 0.4f, targetScale;

        public void Init(float scale)
        {
            sr = GetComponent<SpriteRenderer>();
            targetScale = scale;
        }

        void Update()
        {
            life += Time.deltaTime;
            float t = life / maxLife;
            transform.localScale = Vector3.one * Mathf.Lerp(targetScale * 0.6f, targetScale, t);
            if (sr != null)
            {
                var c = sr.color; c.a = Mathf.Clamp01(1f - t); sr.color = c;
            }
            if (life >= maxLife) Destroy(gameObject);
        }
    }
}
