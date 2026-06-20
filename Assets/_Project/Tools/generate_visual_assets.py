from pathlib import Path
import math
import random

from PIL import Image, ImageDraw, ImageFilter


ROOT = Path(__file__).resolve().parents[3]
OUT = ROOT / "Assets" / "_Project" / "Resources" / "Art"
STORE_OUT = ROOT / "Assets" / "_Project" / "StoreAssets" / "Steam"
GAME_TITLE = "\u8bb0\u5fc6\u5371\u697c"
GAME_TAGLINE = "CARD  |  RUIN  |  MEMORY"


def ensure_dirs():
    for name in ["UI", "Building", "VFX", "Cards", "UID"]:
        (OUT / name).mkdir(parents=True, exist_ok=True)
    STORE_OUT.mkdir(parents=True, exist_ok=True)


def lerp(a, b, t):
    return int(a + (b - a) * t)


def gradient(size, top, bottom):
    width, height = size
    img = Image.new("RGBA", size)
    px = img.load()
    for y in range(height):
        t = y / max(1, height - 1)
        color = tuple(lerp(top[i], bottom[i], t) for i in range(4))
        for x in range(width):
            px[x, y] = color
    return img


def add_noise(img, amount=12, alpha=34, seed=7):
    random.seed(seed)
    noise = Image.new("RGBA", img.size, (0, 0, 0, 0))
    pixels = noise.load()
    for y in range(img.height):
        for x in range(img.width):
            n = random.randint(-amount, amount)
            pixels[x, y] = (max(0, n), max(0, n), max(0, n), alpha)
    return Image.alpha_composite(img, noise)


def rounded_panel(path, size, fill, outline, radius=32, border=3, glow=None):
    img = Image.new("RGBA", size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    rect = (border, border, size[0] - border - 1, size[1] - border - 1)
    if glow:
        glow_img = Image.new("RGBA", size, (0, 0, 0, 0))
        glow_draw = ImageDraw.Draw(glow_img)
        glow_draw.rounded_rectangle(rect, radius=radius, fill=glow)
        glow_img = glow_img.filter(ImageFilter.GaussianBlur(10))
        img = Image.alpha_composite(img, glow_img)
        draw = ImageDraw.Draw(img)
    draw.rounded_rectangle(rect, radius=radius, fill=fill, outline=outline, width=border)
    img.save(path)


def draw_tower(draw, base_x, base_y, scale=1.0, amber=True):
    random.seed(24)
    layers = 10
    for i in range(layers):
        width = int((430 - i * 22) * scale)
        height = int(56 * scale)
        x = base_x - width // 2 + int(math.sin(i * 1.3) * 20 * scale)
        y = base_y - i * int(58 * scale)
        skew = int(((-1) ** i) * random.randint(4, 18) * scale)
        color = (58 + i * 3, 65 + i * 3, 70 + i * 2, 220)
        edge = (150, 158, 158, 130)
        points = [(x + skew, y), (x + width + skew, y + 8), (x + width - skew, y + height), (x - skew, y + height - 8)]
        draw.polygon(points, fill=color)
        draw.line(points + [points[0]], fill=edge, width=max(2, int(2 * scale)))
        for c in range(2):
            cx = x + random.randint(34, max(38, width - 38))
            cy = y + random.randint(12, max(13, height - 12))
            draw.line((cx - 18, cy, cx + 18, cy + random.randint(-8, 8)), fill=(20, 23, 26, 120), width=2)

    if amber:
        for _ in range(32):
            x = base_x + random.randint(-360, 260)
            y = base_y - random.randint(40, 640)
            r = random.randint(2, 5)
            draw.ellipse((x - r, y - r, x + r, y + r), fill=(246, 190, 74, random.randint(90, 190)))


def make_menu_background():
    img = gradient((1920, 1080), (28, 34, 38, 255), (5, 7, 10, 255))
    img = add_noise(img, 8, 18, 11)
    draw = ImageDraw.Draw(img, "RGBA")

    for i in range(18):
        y = 120 + i * 48
        draw.line((0, y, 1920, y + random.randint(-18, 18)), fill=(150, 160, 160, 12), width=1)

    draw.rectangle((0, 0, 760, 1080), fill=(0, 0, 0, 92))
    draw_tower(draw, 1320, 910, 1.18, True)
    draw.rectangle((0, 830, 1920, 1080), fill=(0, 0, 0, 115))
    img = img.filter(ImageFilter.UnsharpMask(radius=1, percent=120, threshold=3))
    img.save(OUT / "UI" / "menu_background.png")


def make_game_background():
    img = gradient((1920, 1080), (20, 26, 29, 255), (8, 10, 12, 255))
    img = add_noise(img, 7, 14, 19)
    draw = ImageDraw.Draw(img, "RGBA")
    for x in range(0, 1920, 92):
        draw.line((x, 150, x - 230, 1080), fill=(119, 136, 143, 16), width=1)
    for y in range(220, 1080, 90):
        draw.line((0, y, 1920, y + random.randint(-20, 20)), fill=(119, 136, 143, 14), width=1)
    draw_tower(draw, 1410, 970, 0.55, False)
    for _ in range(22):
        x = random.randint(80, 1780)
        y = random.randint(140, 850)
        r = random.randint(1, 3)
        draw.ellipse((x - r, y - r, x + r, y + r), fill=(230, 176, 75, 105))
    draw.rectangle((0, 0, 1920, 1080), fill=(0, 0, 0, 38))
    img.save(OUT / "UI" / "game_background.png")


def make_ui_assets():
    rounded_panel(OUT / "UI" / "panel_dark.png", (512, 512), (19, 24, 28, 232), (170, 184, 184, 72), 34, 4)
    rounded_panel(OUT / "UI" / "panel_result.png", (768, 512), (18, 22, 27, 244), (224, 178, 74, 130), 34, 4, (224, 178, 74, 28))
    rounded_panel(OUT / "UI" / "button_idle.png", (512, 128), (42, 52, 60, 246), (188, 198, 196, 90), 22, 4)
    rounded_panel(OUT / "UI" / "button_hover.png", (512, 128), (62, 79, 88, 250), (228, 189, 93, 150), 22, 4, (228, 189, 93, 20))
    rounded_panel(OUT / "UI" / "button_pressed.png", (512, 128), (24, 31, 36, 250), (228, 189, 93, 180), 22, 4)
    rounded_panel(OUT / "UI" / "hud_strip.png", (1024, 160), (13, 17, 20, 236), (126, 142, 150, 60), 22, 3)
    rounded_panel(OUT / "UI" / "hand_tray.png", (1600, 260), (11, 14, 17, 242), (126, 142, 150, 46), 28, 3)


def make_card(path, fill, edge, accent):
    img = Image.new("RGBA", (512, 768), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img, "RGBA")
    draw.rounded_rectangle((14, 14, 498, 754), radius=30, fill=fill, outline=edge, width=6)
    draw.rounded_rectangle((42, 42, 470, 216), radius=22, fill=(255, 255, 255, 22), outline=(255, 255, 255, 38), width=2)
    draw.line((70, 285, 440, 250), fill=accent, width=5)
    draw.line((72, 286, 440, 644), fill=(255, 255, 255, 24), width=2)
    for i in range(8):
        x = 74 + i * 45
        draw.line((x, 472, x + random.randint(-12, 20), 632), fill=(0, 0, 0, 45), width=2)
    for _ in range(10):
        x = random.randint(74, 438)
        y = random.randint(84, 630)
        draw.line((x, y, x + random.randint(-24, 24), y + random.randint(8, 44)), fill=(255, 255, 255, 28), width=1)
    img = add_noise(img, 5, 12, random.randint(1, 9999))
    img.save(path)


def make_cards():
    make_card(OUT / "Cards" / "card_basic.png", (35, 43, 50, 248), (170, 184, 184, 100), (222, 220, 205, 128))
    make_card(OUT / "Cards" / "card_function.png", (33, 50, 70, 248), (170, 190, 218, 120), (114, 162, 205, 160))
    make_card(OUT / "Cards" / "card_oneshot.png", (80, 32, 32, 248), (230, 158, 90, 130), (235, 176, 75, 180))
    make_card(OUT / "Cards" / "card_selected.png", (48, 72, 87, 255), (235, 184, 74, 220), (235, 184, 74, 210))
    make_named_cards()


def make_named_cards():
    specs = {
        "tap": ("basic", (36, 44, 50, 255), (230, 214, 172, 190), draw_tap_icon),
        "strike": ("basic", (39, 43, 48, 255), (238, 160, 82, 205), draw_strike_icon),
        "stabilize": ("basic", (31, 52, 54, 255), (126, 204, 184, 195), draw_stabilize_icon),
        "inspect_crack": ("basic", (33, 42, 50, 255), (176, 200, 218, 195), draw_inspect_icon),
        "cut_support": ("function", (29, 48, 66, 255), (98, 160, 214, 210), draw_cut_support_icon),
        "chain_shock": ("function", (29, 43, 64, 255), (135, 178, 232, 210), draw_chain_icon),
        "sealed_echo": ("function", (47, 42, 55, 255), (240, 190, 84, 220), draw_echo_icon),
        "core_fracture": ("oneshot", (78, 29, 31, 255), (240, 112, 82, 230), draw_core_icon),
    }

    for card_id, (kind, fill, accent, icon_fn) in specs.items():
        edge = (170, 184, 184, 105)
        if kind == "function":
            edge = (150, 190, 220, 135)
        elif kind == "oneshot":
            edge = (236, 150, 88, 165)
        make_named_card(OUT / "Cards" / f"card_{card_id}.png", fill, edge, accent, icon_fn)


def make_named_card(path, fill, edge, accent, icon_fn):
    img = Image.new("RGBA", (512, 768), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img, "RGBA")
    draw.rounded_rectangle((14, 14, 498, 754), radius=34, fill=fill, outline=edge, width=7)
    draw.rounded_rectangle((44, 46, 468, 238), radius=24, fill=(235, 230, 210, 34), outline=(255, 255, 255, 45), width=2)
    draw.rounded_rectangle((56, 272, 456, 618), radius=28, fill=(0, 0, 0, 48), outline=(255, 255, 255, 26), width=2)
    for i in range(7):
        y = 290 + i * 44
        draw.line((78, y, 434, y + random.randint(-18, 18)), fill=(255, 255, 255, 18), width=1)
    icon_fn(draw, accent)
    draw.line((84, 660, 428, 636), fill=accent, width=5)
    draw.line((84, 678, 370, 660), fill=(255, 255, 255, 30), width=2)
    img = add_noise(img, 6, 12, sum(path.name.encode("utf-8")))
    img.save(path)


def draw_crack(draw, points, color, width):
    for a, b in zip(points, points[1:]):
        draw.line((a[0], a[1], b[0], b[1]), fill=color, width=width)


def draw_tap_icon(draw, accent):
    draw.ellipse((190, 350, 322, 482), outline=accent, width=10)
    draw.line((256, 300, 256, 350), fill=accent, width=8)
    draw_crack(draw, [(258, 410), (240, 440), (256, 470), (232, 510)], (18, 20, 22, 210), 7)
    draw.line((258, 410, 288, 442), fill=(255, 255, 255, 80), width=3)


def draw_strike_icon(draw, accent):
    draw.polygon([(246, 302), (330, 414), (278, 420), (338, 560), (178, 400), (236, 402)], fill=accent)
    draw_crack(draw, [(254, 372), (210, 438), (238, 484), (184, 558)], (15, 16, 18, 230), 9)
    draw_crack(draw, [(260, 392), (314, 444), (330, 528)], (15, 16, 18, 190), 6)


def draw_stabilize_icon(draw, accent):
    draw.rounded_rectangle((138, 410, 374, 454), radius=12, fill=accent)
    draw.rounded_rectangle((168, 336, 206, 566), radius=10, fill=(210, 220, 205, 190))
    draw.rounded_rectangle((306, 336, 344, 566), radius=10, fill=(210, 220, 205, 190))
    for y in [366, 500]:
        draw.line((150, y, 360, y - 28), fill=(255, 255, 255, 55), width=5)


def draw_inspect_icon(draw, accent):
    draw.ellipse((150, 318, 330, 498), outline=accent, width=14)
    draw.line((306, 474, 394, 562), fill=accent, width=18)
    draw.line((230, 360, 252, 420), fill=(18, 20, 22, 220), width=6)
    draw.line((252, 420, 228, 478), fill=(18, 20, 22, 220), width=6)
    draw.arc((178, 348, 302, 472), 210, 310, fill=(255, 255, 255, 90), width=5)


def draw_cut_support_icon(draw, accent):
    draw.rounded_rectangle((160, 306, 214, 582), radius=10, fill=(155, 180, 190, 210))
    draw.rounded_rectangle((296, 306, 352, 582), radius=10, fill=(155, 180, 190, 210))
    draw.line((132, 434, 380, 384), fill=accent, width=18)
    draw.line((156, 462, 356, 420), fill=(20, 22, 24, 210), width=8)
    draw.polygon([(244, 396), (286, 432), (250, 458), (214, 420)], fill=(232, 122, 74, 210))


def draw_chain_icon(draw, accent):
    for r, alpha in [(62, 70), (96, 65), (132, 50)]:
        draw.ellipse((256 - r, 438 - r, 256 + r, 438 + r), outline=(accent[0], accent[1], accent[2], alpha), width=8)
    for x in [148, 256, 364]:
        draw.rounded_rectangle((x - 36, 400, x + 36, 476), radius=12, fill=(130, 150, 165, 190), outline=accent, width=5)
        draw.line((x - 20, 438, x + 20, 438), fill=(20, 22, 24, 150), width=4)


def draw_echo_icon(draw, accent):
    for i in range(4):
        inset = i * 24
        draw.arc((132 + inset, 306 + inset, 380 - inset, 554 - inset), 35, 315, fill=(accent[0], accent[1], accent[2], 230 - i * 40), width=9)
    points = []
    cx, cy = 256, 430
    for i in range(10):
        angle = -math.pi / 2 + i * math.pi / 5
        radius = 64 if i % 2 == 0 else 27
        points.append((cx + math.cos(angle) * radius, cy + math.sin(angle) * radius))
    draw.polygon(points, fill=(246, 198, 86, 230), outline=(255, 240, 160, 210))


def draw_core_icon(draw, accent):
    draw.ellipse((162, 320, 350, 508), fill=(108, 24, 28, 230), outline=accent, width=12)
    draw.polygon([(256, 328), (304, 416), (260, 512), (206, 430)], fill=(210, 52, 46, 220))
    draw_crack(draw, [(256, 330), (244, 386), (264, 432), (242, 510)], (15, 10, 12, 230), 8)
    draw_crack(draw, [(260, 420), (320, 452), (344, 502)], (15, 10, 12, 210), 6)
    draw.ellipse((212, 374, 300, 462), outline=(255, 204, 100, 140), width=6)


def make_block(path, fill, edge, cracks=True, glow=None):
    img = Image.new("RGBA", (256, 192), (0, 0, 0, 0))
    if glow:
        glow_img = Image.new("RGBA", (256, 192), (0, 0, 0, 0))
        gd = ImageDraw.Draw(glow_img, "RGBA")
        gd.rounded_rectangle((20, 20, 236, 172), radius=14, fill=glow)
        glow_img = glow_img.filter(ImageFilter.GaussianBlur(12))
        img = Image.alpha_composite(img, glow_img)
    draw = ImageDraw.Draw(img, "RGBA")
    draw.rounded_rectangle((20, 20, 236, 172), radius=14, fill=fill, outline=edge, width=5)
    draw.rectangle((34, 38, 222, 74), fill=(255, 255, 255, 18))
    draw.line((28, 156, 226, 156), fill=(0, 0, 0, 44), width=4)
    if cracks:
        random.seed(sum(path.name.encode("utf-8")))
        for _ in range(5):
            x = random.randint(55, 196)
            y = random.randint(52, 140)
            draw.line((x, y, x + random.randint(-26, 26), y + random.randint(18, 42)), fill=(12, 14, 16, 90), width=3)
    img = add_noise(img, 8, 16, random.randint(10, 5000))
    img.save(path)


def make_building():
    make_block(OUT / "Building" / "block_normal.png", (102, 109, 112, 255), (178, 186, 186, 115))
    make_block(OUT / "Building" / "block_support.png", (44, 80, 112, 255), (142, 182, 213, 140))
    make_block(OUT / "Building" / "block_memory.png", (204, 158, 54, 255), (250, 221, 113, 160), glow=(243, 184, 61, 45))
    make_block(OUT / "Building" / "block_core.png", (136, 39, 37, 255), (242, 139, 92, 170), glow=(220, 68, 51, 55))
    make_block(OUT / "Building" / "block_damaged.png", (70, 74, 77, 255), (154, 161, 162, 105))
    make_block(OUT / "Building" / "block_unstable.png", (183, 93, 36, 255), (252, 181, 84, 180), glow=(238, 127, 45, 44))
    make_block(OUT / "Building" / "block_collapsed.png", (32, 34, 35, 98), (120, 120, 120, 55), cracks=False)


def make_vfx():
    img = Image.new("RGBA", (128, 128), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img, "RGBA")
    cx, cy = 64, 64
    points = []
    for i in range(10):
        angle = -math.pi / 2 + i * math.pi / 5
        radius = 48 if i % 2 == 0 else 20
        points.append((cx + math.cos(angle) * radius, cy + math.sin(angle) * radius))
    glow = Image.new("RGBA", (128, 128), (0, 0, 0, 0))
    gd = ImageDraw.Draw(glow, "RGBA")
    gd.polygon(points, fill=(246, 190, 74, 120))
    glow = glow.filter(ImageFilter.GaussianBlur(10))
    img = Image.alpha_composite(img, glow)
    draw = ImageDraw.Draw(img, "RGBA")
    draw.polygon(points, fill=(246, 200, 91, 245), outline=(255, 238, 150, 230))
    img.save(OUT / "VFX" / "memory_fragment.png")

    warning = Image.new("RGBA", (128, 128), (0, 0, 0, 0))
    draw = ImageDraw.Draw(warning, "RGBA")
    draw.ellipse((18, 18, 110, 110), outline=(237, 119, 54, 220), width=8)
    draw.line((64, 36, 64, 72), fill=(237, 119, 54, 230), width=8)
    draw.ellipse((60, 84, 68, 92), fill=(237, 119, 54, 230))
    warning.save(OUT / "VFX" / "collapse_warning.png")


def try_font(size):
    candidates = [
        "C:/Windows/Fonts/msyh.ttc",
        "C:/Windows/Fonts/simhei.ttf",
        "C:/Windows/Fonts/arial.ttf",
    ]
    for candidate in candidates:
        try:
            from PIL import ImageFont
            return ImageFont.truetype(candidate, size=size)
        except Exception:
            pass
    from PIL import ImageFont
    return ImageFont.load_default()


def fit_text(draw, text, max_width, start_size, min_size=18):
    size = start_size
    while size >= min_size:
        font = try_font(size)
        bbox = draw.textbbox((0, 0), text, font=font)
        if bbox[2] - bbox[0] <= max_width:
            return font
        size -= 2
    return try_font(min_size)


def compose_key_art(size, title=True):
    width, height = size
    base = gradient(size, (25, 31, 35, 255), (4, 6, 9, 255))
    base = add_noise(base, 8, 14, width + height)
    draw = ImageDraw.Draw(base, "RGBA")
    draw.rectangle((0, 0, int(width * 0.45), height), fill=(0, 0, 0, 88))
    scale = max(width / 1920, height / 1080)
    draw_tower(draw, int(width * 0.72), int(height * 0.88), scale * 0.95, True)

    if title:
        title_font = fit_text(draw, "记忆危楼", int(width * 0.38), int(height * 0.13), 22)
        sub_font = fit_text(draw, "CARD  |  RUIN  |  MEMORY", int(width * 0.36), int(height * 0.035), 12)
        draw.text((int(width * 0.07), int(height * 0.32)), "记忆危楼", font=title_font, fill=(238, 232, 213, 255))
        draw.text((int(width * 0.073), int(height * 0.48)), "CARD  |  RUIN  |  MEMORY", font=sub_font, fill=(226, 178, 76, 230))
        draw.line((int(width * 0.075), int(height * 0.55), int(width * 0.36), int(height * 0.55)), fill=(226, 178, 76, 150), width=max(2, int(height * 0.006)))

    return base


def make_steam_assets():
    sizes = {
        "steam_header_capsule_920x430.png": (920, 430),
        "steam_small_capsule_462x174.png": (462, 174),
        "steam_main_capsule_1232x706.png": (1232, 706),
        "steam_vertical_capsule_748x896.png": (748, 896),
        "steam_page_background_1438x810.png": (1438, 810),
        "steam_library_capsule_600x900.png": (600, 900),
        "steam_library_header_920x430.png": (920, 430),
        "steam_library_hero_3840x1240.png": (3840, 1240),
    }

    for filename, size in sizes.items():
        art = compose_key_art(size, title="hero" not in filename)
        if "small_capsule" in filename:
            art = compose_small_capsule(size)
        art.save(STORE_OUT / filename)

    make_logo_asset()


def compose_small_capsule(size):
    width, height = size
    art = gradient(size, (28, 34, 38, 255), (5, 7, 10, 255))
    art = add_noise(art, 5, 12, width + height)
    draw = ImageDraw.Draw(art, "RGBA")
    draw.rectangle((0, 0, width, height), fill=(0, 0, 0, 68))
    draw_tower(draw, int(width * 0.78), int(height * 1.13), 0.13, True)
    font = fit_text(draw, "记忆危楼", int(width * 0.58), 34, 18)
    draw.text((18, 24), "记忆危楼", font=font, fill=(238, 232, 213, 255))
    draw.line((20, 62, 128, 62), fill=(226, 178, 76, 170), width=2)
    return art


def make_logo_asset():
    img = Image.new("RGBA", (1280, 720), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img, "RGBA")
    font = fit_text(draw, "记忆危楼", 920, 190, 80)
    draw.text((140, 210), "记忆危楼", font=font, fill=(238, 232, 213, 255))
    draw.line((160, 450, 950, 450), fill=(226, 178, 76, 210), width=10)
    for x in [1010, 1084, 1158]:
        draw.rounded_rectangle((x, 220, x + 42, 448), radius=10, fill=(70, 80, 84, 210), outline=(230, 218, 180, 110), width=3)
    img.save(STORE_OUT / "steam_logo_transparent_1280x720.png")


def make_uid_style_sheet():
    img = Image.new("RGBA", (1600, 1000), (17, 21, 24, 255))
    draw = ImageDraw.Draw(img, "RGBA")
    title_font = try_font(54)
    body_font = try_font(28)
    small_font = try_font(22)
    draw.text((72, 62), "记忆危楼 UID Style Sheet", font=title_font, fill=(238, 232, 213, 255))
    draw.text((74, 132), "Steam Demo visual direction: quiet ruin, card strategy, warm memory fragments", font=small_font, fill=(195, 202, 198, 255))

    palette = [
        ("Archive Black", (8, 10, 12), "#080A0C"),
        ("Ruin Slate", (35, 43, 50), "#232B32"),
        ("Support Blue", (44, 80, 112), "#2C5070"),
        ("Memory Amber", (226, 178, 76), "#E2B24C"),
        ("Danger Orange", (210, 91, 45), "#D25B2D"),
        ("Core Red", (136, 39, 37), "#882725"),
        ("Paper Text", (238, 232, 213), "#EEE8D5"),
    ]
    x = 72
    for name, rgb, hex_value in palette:
        draw.rounded_rectangle((x, 210, x + 170, 330), radius=16, fill=rgb + (255,), outline=(255, 255, 255, 50), width=2)
        draw.text((x, 350), name, font=small_font, fill=(238, 232, 213, 255))
        draw.text((x, 382), hex_value, font=small_font, fill=(160, 170, 172, 255))
        x += 210

    panel = Image.open(OUT / "UI" / "panel_dark.png").resize((300, 300))
    button = Image.open(OUT / "UI" / "button_idle.png").resize((360, 90))
    card = Image.open(OUT / "Cards" / "card_strike.png").resize((205, 307))
    block = Image.open(OUT / "Building" / "block_memory.png").resize((256, 192))
    img.alpha_composite(panel, (80, 500))
    img.alpha_composite(button, (450, 600))
    img.alpha_composite(card, (870, 500))
    img.alpha_composite(block, (1210, 580))
    draw.text((80, 820), "Panel: sliced dark glass + paper edge", font=body_font, fill=(238, 232, 213, 255))
    draw.text((450, 720), "Button: muted, tactile, amber hover", font=body_font, fill=(238, 232, 213, 255))
    draw.text((870, 840), "Card: symbolic art + concise stats", font=body_font, fill=(238, 232, 213, 255))
    draw.text((1210, 800), "Block: readable state + icon", font=body_font, fill=(238, 232, 213, 255))
    img.save(OUT / "UID" / "uid_style_sheet.png")


def compose_key_art(size, title=True):
    width, height = size
    base = gradient(size, (25, 31, 35, 255), (4, 6, 9, 255))
    base = add_noise(base, 8, 14, width + height)
    draw = ImageDraw.Draw(base, "RGBA")
    draw.rectangle((0, 0, int(width * 0.45), height), fill=(0, 0, 0, 88))
    scale = max(width / 1920, height / 1080)
    draw_tower(draw, int(width * 0.72), int(height * 0.88), scale * 0.95, True)

    if title:
        title_font = fit_text(draw, GAME_TITLE, int(width * 0.38), int(height * 0.13), 22)
        sub_font = fit_text(draw, GAME_TAGLINE, int(width * 0.36), int(height * 0.035), 12)
        draw.text((int(width * 0.07), int(height * 0.32)), GAME_TITLE, font=title_font, fill=(238, 232, 213, 255))
        draw.text((int(width * 0.073), int(height * 0.48)), GAME_TAGLINE, font=sub_font, fill=(226, 178, 76, 230))
        draw.line(
            (int(width * 0.075), int(height * 0.55), int(width * 0.36), int(height * 0.55)),
            fill=(226, 178, 76, 150),
            width=max(2, int(height * 0.006)),
        )

    return base


def compose_small_capsule(size):
    width, height = size
    art = gradient(size, (28, 34, 38, 255), (5, 7, 10, 255))
    art = add_noise(art, 5, 12, width + height)
    draw = ImageDraw.Draw(art, "RGBA")
    draw.rectangle((0, 0, width, height), fill=(0, 0, 0, 68))
    draw_tower(draw, int(width * 0.78), int(height * 1.13), max(0.13, height / 1340), True)
    font = fit_text(draw, GAME_TITLE, int(width * 0.58), max(24, int(height * 0.2)), 18)
    draw.text((int(width * 0.04), int(height * 0.22)), GAME_TITLE, font=font, fill=(238, 232, 213, 255))
    draw.line(
        (int(width * 0.045), int(height * 0.64), int(width * 0.34), int(height * 0.64)),
        fill=(226, 178, 76, 170),
        width=max(2, int(height * 0.018)),
    )
    return art


def make_logo_asset():
    img = Image.new("RGBA", (1280, 720), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img, "RGBA")
    font = fit_text(draw, GAME_TITLE, 920, 190, 80)
    draw.text((140, 210), GAME_TITLE, font=font, fill=(238, 232, 213, 255))
    draw.line((160, 450, 950, 450), fill=(226, 178, 76, 210), width=10)
    for x in [1010, 1084, 1158]:
        draw.rounded_rectangle((x, 220, x + 42, 448), radius=10, fill=(70, 80, 84, 210), outline=(230, 218, 180, 110), width=3)
    img.save(STORE_OUT / "steam_logo_transparent_1280x720.png")


def make_uid_style_sheet():
    img = Image.new("RGBA", (1600, 1000), (17, 21, 24, 255))
    draw = ImageDraw.Draw(img, "RGBA")
    title_font = try_font(54)
    body_font = try_font(28)
    small_font = try_font(22)
    draw.text((72, 62), f"{GAME_TITLE} UID Style Sheet", font=title_font, fill=(238, 232, 213, 255))
    draw.text((74, 132), "Steam Demo visual direction: quiet ruin, card strategy, warm memory fragments", font=small_font, fill=(195, 202, 198, 255))

    palette = [
        ("Archive Black", (8, 10, 12), "#080A0C"),
        ("Ruin Slate", (35, 43, 50), "#232B32"),
        ("Support Blue", (44, 80, 112), "#2C5070"),
        ("Memory Amber", (226, 178, 76), "#E2B24C"),
        ("Danger Orange", (210, 91, 45), "#D25B2D"),
        ("Core Red", (136, 39, 37), "#882725"),
        ("Paper Text", (238, 232, 213), "#EEE8D5"),
    ]
    x = 72
    for name, rgb, hex_value in palette:
        draw.rounded_rectangle((x, 210, x + 170, 330), radius=16, fill=rgb + (255,), outline=(255, 255, 255, 50), width=2)
        draw.text((x, 350), name, font=small_font, fill=(238, 232, 213, 255))
        draw.text((x, 382), hex_value, font=small_font, fill=(160, 170, 172, 255))
        x += 210

    panel = Image.open(OUT / "UI" / "panel_dark.png").resize((300, 300))
    button = Image.open(OUT / "UI" / "button_idle.png").resize((360, 90))
    card = Image.open(OUT / "Cards" / "card_strike.png").resize((205, 307))
    block = Image.open(OUT / "Building" / "block_memory.png").resize((256, 192))
    img.alpha_composite(panel, (80, 500))
    img.alpha_composite(button, (450, 600))
    img.alpha_composite(card, (870, 500))
    img.alpha_composite(block, (1210, 580))
    draw.text((80, 820), "Panel: sliced dark glass + paper edge", font=body_font, fill=(238, 232, 213, 255))
    draw.text((450, 720), "Button: muted, tactile, amber hover", font=body_font, fill=(238, 232, 213, 255))
    draw.text((870, 840), "Card: symbolic art + concise stats", font=body_font, fill=(238, 232, 213, 255))
    draw.text((1210, 800), "Block: readable state + icon", font=body_font, fill=(238, 232, 213, 255))
    img.save(OUT / "UID" / "uid_style_sheet.png")


def main():
    ensure_dirs()
    make_menu_background()
    make_game_background()
    make_ui_assets()
    make_cards()
    make_building()
    make_vfx()
    make_uid_style_sheet()
    make_steam_assets()
    print(f"Generated visual assets in {OUT}")


if __name__ == "__main__":
    main()
