# 🐧 Penguin Sky Smash

A 2D casual arcade **launch & record** game for Android, built in **Unity 6**. A polar-bear slugger bats a hero penguin as far as possible across icy biomes — collect fish coins, bounce off boosters, dodge obstacles, beat your record, and upgrade your gear.

## Tech
- **Engine:** Unity 6000.3.18f1 (2D, URP-free built-in)
- **Target:** Android (landscape, IL2CPP / ARM64, package `com.gadwords.penguinskysmash`)
- **Architecture:** fully **code-driven** — `GameBootstrap` assembles the camera, player, world and UI at runtime; art is loaded from `Resources/`. No manual scene wiring required.

## Project layout
```
Assets/
  Scripts/
    Core/     GameConfig, SaveSystem, AssetDB, GameManager, GameBootstrap
    Player/   LaunchController, PenguinController, BearAnimator
    World/    WorldSpawner, WorldItem, InfiniteScroller, CameraFollow, Fx
    UI/       UIManager, UIFactory, UITex
  Editor/     SpriteImportSettings (auto sprite import), PSSBuild (setup + APK)
  Resources/Art/  Characters, Items, Environment, UI, Effects  (114 sprites)
  Scenes/Game.unity
Tools/
  slice.py          sprite-sheet slicer (flood-fill white bg -> alpha -> components)
  install_assets.py  maps sliced sprites to named game assets
  source/           original art sheets
```

## Build
Open in Unity and use the **PSS** menu:
1. `PSS ▸ 1. Setup Scene and Settings`
2. `PSS ▸ 2. Build Android APK` → `Builds/PenguinSkySmash.apk`

Or headless:
```bash
Unity -batchmode -quit -projectPath . -executeMethod PSS.EditorTools.PSSBuild.BuildAndroid
```

## Gameplay
Tap when the power bar hits the **green (Perfect)** zone → the bear swings → the penguin flies. Collect coins, chain boosters for combos, and spend Fish Coins on 5 upgrades (Bat Power, Glide, Bounce, Coin Magnet, Rocket Duration).
