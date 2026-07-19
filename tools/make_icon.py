"""
Build MultiRoblox icon assets that stay crisp from 16px (taskbar) to 256px
(Start Menu / desktop). Each size is drawn from scratch so title bars and
window silhouettes stay readable after Windows DPI scaling.
"""
from __future__ import annotations

import io
import struct
from pathlib import Path

from PIL import Image, ImageDraw, ImageFilter

ROOT = Path(__file__).resolve().parent.parent
ASSETS = ROOT / "assets"
DOCS = ROOT / "docs"

BG = (24, 26, 32, 255)
WIN_FRONT = (64, 156, 255, 255)
WIN_BACK = (109, 179, 255, 255)
TITLE = (245, 248, 255, 255)
SHADOW = (0, 0, 0, 100)


def draw_window(base: Image.Image, box, radius: int, fill, title_h: int, shadow: bool):
    size = base.size[0]
    layer = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    d = ImageDraw.Draw(layer)

    if shadow and size >= 24:
        sx = max(1, round(size * 0.025))
        sy = max(1, round(size * 0.035))
        sbox = [box[0] + sx, box[1] + sy, box[2] + sx, box[3] + sy]
        ImageDraw.Draw(layer).rounded_rectangle(sbox, radius=radius, fill=SHADOW)
        blur = max(1, round(size * 0.04))
        layer = layer.filter(ImageFilter.GaussianBlur(blur))
        base.alpha_composite(layer)
        layer = Image.new("RGBA", (size, size), (0, 0, 0, 0))
        d = ImageDraw.Draw(layer)

    d.rounded_rectangle(box, radius=radius, fill=fill)

    # Title bar clipped to the rounded window via a mask.
    mask = Image.new("L", (size, size), 0)
    ImageDraw.Draw(mask).rounded_rectangle(box, radius=radius, fill=255)
    title = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    ImageDraw.Draw(title).rectangle(
        [box[0], box[1], box[2], min(box[1] + title_h, box[3])],
        fill=TITLE,
    )
    # Keep only pixels inside the window silhouette.
    tr, tg, tb, ta = title.split()
    from PIL import ImageChops
    ta = ImageChops.multiply(ta, mask)
    title = Image.merge("RGBA", (tr, tg, tb, ta))
    layer.alpha_composite(title)

    base.alpha_composite(layer)


def draw_icon(size: int) -> Image.Image:
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)

    # Transparent margin so Start Menu / taskbar don't clip the rounded tile.
    margin = max(1, round(size * 0.05))
    tile = [margin, margin, size - margin - 1, size - margin - 1]
    tile_r = max(2, round(size * 0.22))
    draw.rounded_rectangle(tile, radius=tile_r, fill=BG)

    # Landscape windows (wider than tall) with a thick enough title bar that
    # still reads at 16px. Tiny sizes get less padding and a bigger mark.
    if size <= 20:
        pad = size * 0.16
        dx, dy = size * 0.16, size * 0.14
        win_r = max(1, round(size * 0.08))
        title_h = max(2, round(size * 0.14))
        win_h = size * 0.42
    elif size <= 32:
        pad = size * 0.15
        dx, dy = size * 0.17, size * 0.15
        win_r = max(2, round(size * 0.10))
        title_h = max(3, round(size * 0.13))
        win_h = size * 0.40
    else:
        pad = size * 0.14
        dx, dy = size * 0.18, size * 0.16
        win_r = max(3, round(size * 0.11))
        title_h = max(4, round(size * 0.12))
        win_h = size * 0.38

    # Shared left/right span for the pair, then offset them.
    left = margin + pad
    right = size - margin - pad
    win_w = (right - left) - dx
    top0 = margin + pad + size * 0.02

    back = [left, top0, left + win_w, top0 + win_h]
    front = [left + dx, top0 + dy, left + dx + win_w, top0 + dy + win_h]

    draw_window(img, back, win_r, WIN_BACK, title_h, shadow=False)
    draw_window(img, front, win_r, WIN_FRONT, title_h, shadow=True)
    return img


def write_ico(path: Path, sizes: list[int]):
    images = {s: draw_icon(s) for s in sizes}
    entries = []
    blobs = []
    for size in sorted(images):
        buf = io.BytesIO()
        images[size].save(buf, format="PNG")
        data = buf.getvalue()
        w = 0 if size >= 256 else size
        h = 0 if size >= 256 else size
        entries.append((w, h, len(data)))
        blobs.append(data)

    count = len(entries)
    header = struct.pack("<HHH", 0, 1, count)
    offset = 6 + 16 * count
    directory = b""
    for (w, h, nbytes) in entries:
        directory += struct.pack("<BBBBHHII", w, h, 0, 0, 1, 32, nbytes, offset)
        offset += nbytes
    path.write_bytes(header + directory + b"".join(blobs))


def main():
    ASSETS.mkdir(exist_ok=True)
    DOCS.mkdir(exist_ok=True)

    master = draw_icon(1024)
    master.save(ASSETS / "MultiRoblox.png", format="PNG", optimize=True)
    print("wrote assets/MultiRoblox.png")

    # Sizes Windows actually requests for taskbar, Start, Alt-Tab, desktop.
    ico_sizes = [16, 20, 24, 32, 40, 48, 64, 128, 256]
    write_ico(ASSETS / "MultiRoblox.ico", ico_sizes)
    print(f"wrote assets/MultiRoblox.ico ({', '.join(map(str, ico_sizes))})")

    draw_icon(64).save(DOCS / "favicon.png", format="PNG", optimize=True)
    draw_icon(192).save(DOCS / "icon-192.png", format="PNG", optimize=True)
    print("wrote docs/favicon.png, docs/icon-192.png")

    # Side-by-side preview for visual QA
    gap = 10
    width = sum(ico_sizes) + gap * (len(ico_sizes) + 1)
    preview = Image.new("RGBA", (width, max(ico_sizes) + gap * 2), (48, 50, 56, 255))
    x = gap
    for s in ico_sizes:
        preview.paste(draw_icon(s), (x, gap), draw_icon(s))
        x += s + gap
    preview.save(ASSETS / "icon-preview.png")
    print("wrote assets/icon-preview.png")


if __name__ == "__main__":
    main()
