using System.Collections.Generic;
using UnityEngine;

namespace PSS
{
    /// Loads every sprite under Resources/Art once and exposes them by file name.
    public static class AssetDB
    {
        static Dictionary<string, Sprite> _sprites;

        static readonly string[] Folders =
        {
            "Art/Characters", "Art/Items", "Art/Environment", "Art/UI", "Art/Effects"
        };

        public static void Init()
        {
            _sprites = new Dictionary<string, Sprite>();
            foreach (var folder in Folders)
                foreach (var s in Resources.LoadAll<Sprite>(folder))
                    _sprites[s.name] = s;
            Debug.Log($"[AssetDB] loaded {_sprites.Count} sprites");
        }

        public static Sprite S(string name)
        {
            if (_sprites == null) Init();
            if (_sprites.TryGetValue(name, out var s)) return s;
            Debug.LogWarning($"[AssetDB] sprite not found: {name}");
            return null;
        }

        public static bool Has(string name)
        {
            if (_sprites == null) Init();
            return _sprites.ContainsKey(name);
        }

        // Bats available as skins / bat-power visuals
        public static readonly string[] Bats =
            { "bat_wood", "bat_hockey", "bat_pan", "bat_hammer", "bat_fish", "bat_golden" };
    }
}
