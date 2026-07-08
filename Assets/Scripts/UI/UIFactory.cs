using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace PSS
{
    /// Helpers to build UGUI screens fully from code (legacy Text -> no TMP essentials needed).
    public static class UIFactory
    {
        static Font _font;
        public static Font UIFont
        {
            get
            {
                if (_font == null)
                {
                    _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    if (_font == null) _font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                }
                return _font;
            }
        }

        public static readonly Color Blue   = new Color(0.16f, 0.55f, 0.95f);
        public static readonly Color Navy    = new Color(0.09f, 0.22f, 0.42f);
        public static readonly Color Green  = new Color(0.30f, 0.78f, 0.30f);
        public static readonly Color Gold   = new Color(1f, 0.82f, 0.24f);
        public static readonly Color White  = Color.white;

        public static Canvas CreateCanvas(string name, int order)
        {
            var go = new GameObject(name);
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = order;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        public static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        public static RectTransform Rect(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        public static void SetAnchors(RectTransform rt, Vector2 aMin, Vector2 aMax, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = aMin; rt.anchorMax = aMax; rt.pivot = pivot;
            rt.anchoredPosition = pos; rt.sizeDelta = size;
        }

        public static RectTransform FullScreen(Transform parent, string name)
        {
            var rt = Rect(parent, name);
            SetAnchors(rt, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            return rt;
        }

        public static Image Panel(Transform parent, string name, Color color)
        {
            var rt = FullScreen(parent, name);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            return img;
        }

        /// A clean rounded-rect box (generated sprite, 9-sliced).
        public static Image RoundedBox(Transform parent, string name, Color fill, int radius,
                                       Vector2 aMin, Vector2 aMax, Vector2 pivot, Vector2 pos, Vector2 size,
                                       Color border = default, int borderW = 0)
        {
            var rt = Rect(parent, name);
            SetAnchors(rt, aMin, aMax, pivot, pos, size);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = UITex.Rounded(radius, fill, border, borderW);
            img.type = Image.Type.Sliced;
            img.raycastTarget = false;
            return img;
        }

        public static Image SpriteImage(Transform parent, string spriteName, Vector2 aMin, Vector2 aMax, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            var rt = Rect(parent, spriteName);
            SetAnchors(rt, aMin, aMax, pivot, pos, size);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = AssetDB.S(spriteName);
            img.raycastTarget = false;
            if (img.sprite != null) img.preserveAspect = true;
            return img;
        }

        public static Text Label(Transform parent, string text, int size, Color color,
                                 TextAnchor align, Vector2 aMin, Vector2 aMax, Vector2 pivot, Vector2 pos, Vector2 sz)
        {
            var rt = Rect(parent, "Label");
            SetAnchors(rt, aMin, aMax, pivot, pos, sz);
            var t = rt.gameObject.AddComponent<Text>();
            t.font = UIFont; t.text = text; t.fontSize = size; t.color = color;
            t.alignment = align; t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow; t.fontStyle = FontStyle.Bold;
            t.raycastTarget = false;
            var sh = rt.gameObject.AddComponent<Shadow>();
            sh.effectColor = new Color(0, 0, 0, 0.45f); sh.effectDistance = new Vector2(2, -2);
            return t;
        }

        /// Sprite-backed button with a centered label. Returns the Button; out label for updates.
        public static Button Button(Transform parent, string spriteName, string label, int labelSize,
                                    Vector2 aMin, Vector2 aMax, Vector2 pivot, Vector2 pos, Vector2 size, UnityAction onClick)
        {
            var rt = Rect(parent, "btn_" + label);
            SetAnchors(rt, aMin, aMax, pivot, pos, size);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = AssetDB.S(spriteName);
            img.type = Image.Type.Sliced;   // 9-slice: button art has borders set on import
            img.preserveAspect = false;
            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.transition = Selectable.Transition.ColorTint;
            var cb = btn.colors; cb.highlightedColor = new Color(1.05f, 1.05f, 1.05f);
            cb.pressedColor = new Color(0.85f, 0.85f, 0.85f); btn.colors = cb;
            if (onClick != null) btn.onClick.AddListener(onClick);

            if (!string.IsNullOrEmpty(label))
            {
                var l = Label(rt, label, labelSize, White, TextAnchor.MiddleCenter,
                    Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), new Vector2(0, 6), Vector2.zero);
            }
            return btn;
        }
    }
}
