#!/usr/bin/env python3
"""
Penguin Sky Smash — sprite sheet slicer.
Sheets are flat RGB on a near-white (~249) background (no real alpha).
Strategy:
  1. Build a near-white background mask (high luminance + low saturation).
  2. Flood-fill background from the image border -> "exterior" background only.
     Interior white (e.g. penguin belly enclosed by dark outline) stays opaque.
  3. alpha = 0 where exterior background, else 255.
  4. Connected-components on the opaque mask -> one component per sprite.
  5. Filter by area, tight-crop each sprite, export transparent PNG + a labeled montage.
"""
import os, sys, json
import numpy as np
from PIL import Image, ImageDraw, ImageFont
from scipy import ndimage

SRC = os.path.join(os.path.dirname(__file__), "source")
OUT = os.path.join(os.path.dirname(__file__), "sliced")
MON = os.path.join(os.path.dirname(__file__), "montage")
os.makedirs(OUT, exist_ok=True)
os.makedirs(MON, exist_ok=True)

# Per-sheet config. lum_bg = luminance above which pixel *may* be background.
# sat_bg = max saturation to still count as background (colored pixels are never bg).
# min_area = ignore final blobs smaller than this many px.
# noise = drop mask specks smaller than this BEFORE reconnecting (kills bridges).
# close = binary_closing iters — reconnects broken outlines w/o merging neighbors.
CFG = {
    "sheet_bear":       dict(lum_bg=238, sat_bg=16, min_area=2500, noise=200, close=3),
    "sheet_penguin":    dict(lum_bg=238, sat_bg=16, min_area=2500, noise=200, close=3),
    "sheet_npcs":       dict(lum_bg=232, sat_bg=16, min_area=2200, noise=250, close=3),
    "sheet_bats":       dict(lum_bg=232, sat_bg=16, min_area=1500, noise=250, close=3),
    "sheet_pickups":    dict(lum_bg=236, sat_bg=16, min_area=1500, noise=200, close=3),
    "sheet_obstacles":  dict(lum_bg=236, sat_bg=16, min_area=1800, noise=200, close=3),
    "sheet_effects":    dict(lum_bg=240, sat_bg=14, min_area=900,  noise=120, close=2),
    "sheet_ui":         dict(lum_bg=234, sat_bg=16, min_area=1500, noise=200, close=3),
    "sheet_tiles":      dict(lum_bg=236, sat_bg=16, min_area=1800, noise=200, close=3),
}

def make_alpha(rgb, lum_bg, sat_bg):
    r = rgb[:,:,0].astype(np.int16); g = rgb[:,:,1].astype(np.int16); b = rgb[:,:,2].astype(np.int16)
    lum = (r+g+b)/3.0
    sat = np.maximum(np.maximum(r,g),b) - np.minimum(np.minimum(r,g),b)
    near_white = (lum >= lum_bg) & (sat <= sat_bg)
    # exterior background = near-white region connected to the border
    lbl, n = ndimage.label(near_white)
    border = set(np.unique(np.concatenate([lbl[0,:], lbl[-1,:], lbl[:,0], lbl[:,-1]])))
    border.discard(0)
    exterior = np.isin(lbl, list(border))
    alpha = np.where(exterior, 0, 255).astype(np.uint8)
    return alpha

def slice_sheet(name, cfg):
    path = os.path.join(SRC, name + ".png")
    rgb = np.array(Image.open(path).convert("RGB"))
    H, W, _ = rgb.shape
    alpha = make_alpha(rgb, cfg["lum_bg"], cfg["sat_bg"])
    opaque = alpha > 0
    # 1) kill tiny noise specks (sub-threshold compression halos) so they can't bridge sprites
    lbl0, n0 = ndimage.label(opaque)
    if n0:
        sizes = np.bincount(lbl0.ravel())
        keep = sizes >= cfg["noise"]; keep[0] = False
        opaque = keep[lbl0]
    # 2) close small gaps to reconnect broken outlines without merging neighbors
    grown = ndimage.binary_closing(opaque, iterations=cfg["close"])
    lbl, n = ndimage.label(grown)
    boxes = ndimage.find_objects(lbl)
    sprites = []
    for i, sl in enumerate(boxes, start=1):
        if sl is None: continue
        ys, xs = sl
        comp = (lbl[ys, xs] == i)
        area = int(comp.sum())
        if area < cfg["min_area"]:
            continue
        y0,y1,x0,x1 = ys.start, ys.stop, xs.start, xs.stop
        boxes_area = (y1-y0)*(x1-x0)
        sprites.append(dict(x0=x0,y0=y0,x1=x1,y1=y1,area=area,bbox=boxes_area))
    # reading order: row-band by y then x
    sprites.sort(key=lambda s:(round(s["y0"]/60), s["x0"]))
    outdir = os.path.join(OUT, name); os.makedirs(outdir, exist_ok=True)
    for f in os.listdir(outdir): os.remove(os.path.join(outdir,f))
    rgba = np.dstack([rgb, alpha])
    meta = []
    for idx, s in enumerate(sprites):
        pad = 6
        x0=max(0,s["x0"]-pad); y0=max(0,s["y0"]-pad); x1=min(W,s["x1"]+pad); y1=min(H,s["y1"]+pad)
        crop = rgba[y0:y1, x0:x1]
        Image.fromarray(crop,"RGBA").save(os.path.join(outdir, f"{name}_{idx:02d}.png"))
        meta.append(dict(idx=idx, x=x0,y=y0,w=x1-x0,h=y1-y0,area=s["area"]))
    build_montage(name, outdir, meta)
    print(f"{name:18s} -> {len(meta):2d} sprites  (image {W}x{H})")
    return meta

def build_montage(name, outdir, meta):
    if not meta: return
    cols = 5
    rows = (len(meta)+cols-1)//cols
    cell = 220
    mon = Image.new("RGBA",(cols*cell, rows*cell),(40,44,52,255))
    dr = ImageDraw.Draw(mon)
    try: font = ImageFont.truetype("/System/Library/Fonts/Supplemental/Arial Bold.ttf", 26)
    except: font = ImageFont.load_default()
    for m in meta:
        sp = Image.open(os.path.join(outdir, f"{name}_{m['idx']:02d}.png")).convert("RGBA")
        s = min((cell-40)/sp.width,(cell-40)/sp.height,1.0)
        sp2 = sp.resize((max(1,int(sp.width*s)),max(1,int(sp.height*s))))
        cx = (m["idx"]%cols)*cell; cy=(m["idx"]//cols)*cell
        # checker so transparency is visible
        for yy in range(cy,cy+cell,20):
            for xx in range(cx,cx+cell,20):
                if ((xx//20)+(yy//20))%2==0:
                    dr.rectangle([xx,yy,xx+20,yy+20],fill=(70,74,82,255))
        mon.alpha_composite(sp2,(cx+(cell-sp2.width)//2, cy+(cell-sp2.height)//2))
        dr.rectangle([cx,cy,cx+cell-1,cy+cell-1],outline=(120,180,255,255),width=2)
        dr.text((cx+8,cy+6),f"#{m['idx']}",font=font,fill=(255,220,80,255))
        dr.text((cx+8,cy+cell-30),f"{m['w']}x{m['h']}",font=font,fill=(200,200,200,255))
    mon.save(os.path.join(MON,f"{name}_montage.png"))

def slice_backgrounds():
    """4 seamless biomes stacked vertically."""
    path = os.path.join(SRC,"sheet_backgrounds.png")
    rgb = np.array(Image.open(path).convert("RGB"))
    H,W,_ = rgb.shape
    names = ["bg_arctic","bg_forest","bg_northern_lights","bg_autumn"]
    band = H//4
    outdir = os.path.join(OUT,"backgrounds"); os.makedirs(outdir,exist_ok=True)
    for f in os.listdir(outdir): os.remove(os.path.join(outdir,f))
    for i,nm in enumerate(names):
        y0=i*band; y1=(i+1)*band if i<3 else H
        Image.fromarray(rgb[y0:y1]).save(os.path.join(outdir,f"{nm}.png"))
    print(f"backgrounds        -> 4 biomes ({W}x{band} each)")

if __name__=="__main__":
    only = sys.argv[1] if len(sys.argv)>1 else None
    all_meta={}
    for name,cfg in CFG.items():
        if only and only!=name: continue
        all_meta[name]=slice_sheet(name,cfg)
    if not only or only=="backgrounds":
        slice_backgrounds()
    json.dump(all_meta, open(os.path.join(OUT,"_meta.json"),"w"), indent=1)
    print("\nMontages in:", MON)
