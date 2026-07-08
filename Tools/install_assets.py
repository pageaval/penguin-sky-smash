#!/usr/bin/env python3
"""Copy sliced sprites into Assets/Art with semantic game names."""
import os, shutil
ROOT = os.path.dirname(os.path.dirname(__file__))
SL = os.path.join(ROOT, "Tools", "sliced")
ART = os.path.join(ROOT, "Assets", "Resources", "Art")

# (sheet, index) -> (subfolder, name)
MAP = {
    # ---- Penguin (hero projectile) ----
    ("sheet_penguin",0):("Characters","penguin_idle"),
    ("sheet_penguin",1):("Characters","penguin_dive"),
    ("sheet_penguin",2):("Characters","penguin_angry"),
    ("sheet_penguin",3):("Characters","penguin_fly_fast"),
    ("sheet_penguin",4):("Characters","penguin_spin"),
    ("sheet_penguin",5):("Characters","penguin_slide"),
    ("sheet_penguin",6):("Characters","penguin_point"),
    ("sheet_penguin",7):("Characters","penguin_splash"),
    ("sheet_penguin",8):("Characters","penguin_dizzy"),
    ("sheet_penguin",9):("Characters","penguin_frozen"),
    ("sheet_penguin",10):("Characters","penguin_celebrate"),
    ("sheet_penguin",11):("Characters","penguin_ko"),
    # ---- Bear (slugger) ----
    ("sheet_bear",0):("Characters","bear_idle"),
    ("sheet_bear",1):("Characters","bear_talk"),
    ("sheet_bear",2):("Characters","bear_ready"),
    ("sheet_bear",3):("Characters","bear_aim"),
    ("sheet_bear",4):("Characters","bear_point"),
    ("sheet_bear",5):("Characters","bear_swing1"),
    ("sheet_bear",6):("Characters","bear_cheer"),
    ("sheet_bear",7):("Characters","bear_swing_hit"),
    ("sheet_bear",8):("Characters","bear_swing_impact"),
    ("sheet_bear",9):("Characters","bear_victory"),
    # ---- NPCs / creatures ----
    ("sheet_npcs",0):("Characters","fan_penguin_1"),
    ("sheet_npcs",1):("Characters","fan_penguin_2"),
    ("sheet_npcs",2):("Characters","fan_penguin_3"),
    ("sheet_npcs",3):("Characters","cheer_penguin_1"),
    ("sheet_npcs",4):("Characters","cheer_penguin_2"),
    ("sheet_npcs",5):("Characters","seal_bumper"),
    ("sheet_npcs",6):("Characters","seal_bumper_2"),
    ("sheet_npcs",7):("Characters","walrus_1"),
    ("sheet_npcs",8):("Characters","walrus_2"),
    ("sheet_npcs",9):("Characters","moose_obstacle"),
    ("sheet_npcs",10):("Characters","moose_obstacle_2"),
    ("sheet_npcs",11):("Characters","snowman_1"),
    ("sheet_npcs",12):("Characters","snowman_2"),
    # ---- Bats / weapons ----
    ("sheet_bats",0):("Items","bat_wood"),
    ("sheet_bats",1):("Items","bat_hockey"),
    ("sheet_bats",2):("Items","bat_pan"),
    ("sheet_bats",3):("Items","bat_hammer"),
    ("sheet_bats",4):("Items","bat_fish"),
    ("sheet_bats",5):("Items","bat_golden"),
    ("sheet_bats",6):("Items","bumper_blue"),
    ("sheet_bats",7):("Items","bumper_spring"),
    ("sheet_bats",8):("Items","target"),
    ("sheet_bats",9):("Items","fx_star_hit"),
    # ---- Pickups ----
    ("sheet_pickups",0):("Items","coin_paw"),
    ("sheet_pickups",1):("Items","coin_fish"),
    ("sheet_pickups",2):("Items","fish_blue"),
    ("sheet_pickups",3):("Items","fish_gold"),
    ("sheet_pickups",4):("Items","rocket_boost"),
    ("sheet_pickups",5):("Items","chest"),
    ("sheet_pickups",6):("Items","speed_chevron"),
    ("sheet_pickups",7):("Items","magnet"),
    ("sheet_pickups",8):("Items","shield_bubble"),
    ("sheet_pickups",9):("Items","heart"),
    ("sheet_pickups",10):("Items","star_bonus"),
    ("sheet_pickups",11):("Items","gift"),
    # ---- Obstacles ----
    ("sheet_obstacles",0):("Environment","ice_block_low"),
    ("sheet_obstacles",1):("Environment","ice_wall"),
    ("sheet_obstacles",2):("Environment","snow_pile"),
    ("sheet_obstacles",3):("Environment","spike_ball"),
    ("sheet_obstacles",4):("Environment","mushroom_bounce"),
    ("sheet_obstacles",5):("Environment","crate_wood"),
    ("sheet_obstacles",6):("Environment","ice_puddle"),
    ("sheet_obstacles",7):("Environment","sign_fish"),
    ("sheet_obstacles",8):("Environment","spring_pad"),
    ("sheet_obstacles",9):("Environment","stone_gem"),
    ("sheet_obstacles",10):("Environment","spring_pad_blue"),
    ("sheet_obstacles",11):("Environment","ice_hole"),
    # ---- UI ----
    ("sheet_ui",0):("UI","btn_blue_long"),
    ("sheet_ui",1):("UI","btn_pause"),
    ("sheet_ui",2):("UI","coin_counter"),
    ("sheet_ui",3):("UI","btn_blue_wide"),   # clean wide button (was mislabeled small)
    ("sheet_ui",4):("UI","btn_settings"),
    ("sheet_ui",5):("UI","distance_bar"),    # 123M progress bar w/ flag (was mislabeled wide button)
    ("sheet_ui",6):("UI","combo_badge"),
    ("sheet_ui",7):("UI","btn_shop"),
    ("sheet_ui",8):("UI","flag_marker"),
    ("sheet_ui",9):("UI","results_panel"),
    ("sheet_ui",10):("UI","ribbon_banner"),
    # ---- Effects ----
    ("sheet_effects",0):("Effects","fx_star_burst"),
    ("sheet_effects",1):("Effects","fx_snow_puff"),
    ("sheet_effects",2):("Effects","fx_splash"),
    ("sheet_effects",3):("Effects","fx_speed_lines"),
    ("sheet_effects",4):("Effects","fx_dot"),
    ("sheet_effects",5):("Effects","fx_impact_burst"),
    ("sheet_effects",6):("Effects","fx_sparkle"),
    ("sheet_effects",7):("Effects","fx_fireball"),
    ("sheet_effects",8):("Effects","fx_ice_crystal"),
    ("sheet_effects",9):("Effects","fx_speed_arc"),
    ("sheet_effects",10):("Effects","fx_star_spin"),
    ("sheet_effects",11):("Effects","fx_wind"),
    ("sheet_effects",12):("Effects","fx_star"),
    ("sheet_effects",13):("Effects","fx_coin_sparkle"),
    ("sheet_effects",14):("Effects","fx_ripple"),
    ("sheet_effects",15):("Effects","fx_speed_trail"),
    # ---- Tiles / environment ----
    ("sheet_tiles",0):("Environment","ground_ice"),
    ("sheet_tiles",1):("Environment","ice_border"),
    ("sheet_tiles",2):("Environment","tree_pine_tall"),
    ("sheet_tiles",3):("Environment","tree_pine_snow"),
    ("sheet_tiles",4):("Environment","ice_platform"),
    ("sheet_tiles",5):("Environment","ground_grass"),
    ("sheet_tiles",6):("Environment","mushroom_red"),
    ("sheet_tiles",7):("Environment","igloo"),
    ("sheet_tiles",8):("Environment","crystal_blue"),
    ("sheet_tiles",9):("Environment","mushroom_purple"),
    ("sheet_tiles",10):("Environment","mushroom_pink"),
    ("sheet_tiles",11):("Environment","sign_wood"),
    ("sheet_tiles",12):("Environment","stump"),
    ("sheet_tiles",13):("Environment","rock_ice"),
}

def main():
    copied = 0; missing = []
    for (sheet, idx), (sub, name) in MAP.items():
        src = os.path.join(SL, sheet, f"{sheet}_{idx:02d}.png")
        if not os.path.exists(src):
            missing.append(src); continue
        dst = os.path.join(ART, sub, f"{name}.png")
        shutil.copy2(src, dst); copied += 1
    # backgrounds
    bgsrc = os.path.join(SL, "backgrounds")
    for f in os.listdir(bgsrc):
        shutil.copy2(os.path.join(bgsrc,f), os.path.join(ART,"Environment",f)); copied += 1
    print(f"Copied {copied} sprites into Assets/Art")
    if missing:
        print("MISSING:", *missing, sep="\n  ")
    for sub in ["Characters","Items","Environment","UI","Effects"]:
        n = len([x for x in os.listdir(os.path.join(ART,sub)) if x.endswith('.png')])
        print(f"  Art/{sub}: {n} png")

if __name__=="__main__":
    main()
