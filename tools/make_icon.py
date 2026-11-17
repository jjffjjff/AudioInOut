"""Draw the I\\O app icon geometrically (no font) and write the app's icon assets.

Usage: py tools/make_icon.py   (needs Pillow)
"""
import math
from pathlib import Path
from PIL import Image, ImageDraw

ASSETS = Path(__file__).resolve().parent.parent / "AudioInOut" / "Assets"
N = 1024  # master canvas
SIZES = [16, 20, 24, 32, 40, 48, 64, 128, 256]


def master(stroke=0.068, cap=0.30, gap=0.085, gap2=0.062, slant=0.08, margin=0.012, radius=0.225):
    S = 4 * N  # supersample
    img = Image.new("RGBA", (S, S), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)

    # tile: soft vertical gradient, near-black
    m = margin * S
    top, bot = (38, 38, 42), (8, 8, 10)
    grad = Image.linear_gradient("L").resize((S, S))
    tile = Image.composite(Image.new("RGBA", (S, S), bot + (255,)),
                           Image.new("RGBA", (S, S), top + (255,)), grad)
    mask = Image.new("L", (S, S), 0)
    ImageDraw.Draw(mask).rounded_rectangle([m, m, S - m, S - m], radius=radius * S, fill=255)
    img.paste(tile, (0, 0), mask)

    w = stroke * S          # stem width
    h = cap * S             # cap height
    D = h * 1.03            # O overshoots cap height slightly
    dx = slant * S          # slash horizontal run
    th = w * 0.92 / math.sin(math.atan2(h, dx))  # slash horizontal thickness (diagonals read heavier)
    G, G2 = gap * S, gap2 * S  # I-slash and slash-O gaps, measured at mid-height

    total = w + G + th + G2 + D
    x = (S - total) / 2 + 0.006 * S   # nudge right: round O reads lighter than straight I
    yt, yb = (S - h) / 2, (S + h) / 2
    white = (255, 255, 255, 255)

    # I
    d.rectangle([x, yt, x + w, yb], fill=white)
    x += w + G
    # backslash, centred on its mid-height slot
    cx = x + th / 2
    d.polygon([(cx - dx / 2 - th / 2, yt), (cx - dx / 2 + th / 2, yt),
               (cx + dx / 2 + th / 2, yb), (cx + dx / 2 - th / 2, yb)], fill=white)
    x += th + G2
    # O ring: white disc, hole punched back to tile colour
    cy, r = S / 2, D / 2
    d.ellipse([x, cy - r, x + D, cy + r], fill=white)
    ri, c = r - w * 1.02, x + r
    hole = Image.new("L", (S, S), 0)
    ImageDraw.Draw(hole).ellipse([c - ri, cy - ri, c + ri, cy + ri], fill=255)
    img.paste(tile, (0, 0), hole)
    return img.resize((N, N), Image.LANCZOS)


if __name__ == "__main__":
    big = master()
    # small sizes: bolder, larger glyph so strokes survive downsampling
    small = master(stroke=0.105, cap=0.42, gap=0.10, gap2=0.075, slant=0.10, radius=0.2)
    frames = [(small if s <= 32 else big).resize((s, s), Image.LANCZOS) for s in SIZES]
    frames[-1].save(ASSETS / "AppIcon.png")
    for name in ("Icon-Light.ico", "Icon-Dark.ico"):
        frames[-1].save(ASSETS / name, sizes=[(s, s) for s in SIZES], append_images=frames[:-1])
