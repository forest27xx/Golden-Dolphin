from pathlib import Path
import math
import random

from PIL import Image, ImageDraw, ImageFilter, ImageChops


ROOT = Path(__file__).resolve().parents[3]
OUT = ROOT / "Assets" / "_Project" / "Resources" / "Art" / "Cards" / "AgentIllustrations"

WIDTH = 512
HEIGHT = 768
SCALE = 3

ARCHIVE_BLACK = (7, 9, 11, 255)
RUIN_TOP = (34, 39, 42, 255)
RUIN_BOTTOM = (13, 15, 18, 255)
STONE = (83, 89, 88, 255)
STONE_DARK = (33, 37, 39, 255)
PAPER = (220, 208, 176, 255)
MEMORY_GOLD = (232, 178, 72, 255)
MEMORY_LIGHT = (255, 222, 134, 255)
SUPPORT_BLUE = (86, 129, 139, 255)
CORE_RED = (198, 46, 42, 255)
CORE_DARK = (83, 22, 24, 255)


def sc(value):
    return int(round(value * SCALE))


def rgba(color, alpha=None):
    if len(color) == 4:
        if alpha is None:
            return color
        return color[:3] + (alpha,)
    if alpha is None:
        return color + (255,)
    return color + (alpha,)


def sbox(box):
    return tuple(sc(v) for v in box)


def spoints(points):
    return [(sc(x), sc(y)) for x, y in points]


def gradient(size, top, bottom):
    width, height = size
    img = Image.new("RGBA", size)
    px = img.load()
    for y in range(height):
        t = y / max(1, height - 1)
        color = tuple(int(top[i] + (bottom[i] - top[i]) * t) for i in range(4))
        for x in range(width):
            px[x, y] = color
    return img


def add_texture(img, seed):
    rng = random.Random(seed)

    light = Image.effect_noise(img.size, 34).convert("L")
    light_alpha = light.point(lambda p: max(0, min(24, int(abs(p - 128) * 0.28))))
    light_layer = Image.new("RGBA", img.size, (230, 224, 204, 0))
    light_layer.putalpha(light_alpha)
    img = Image.alpha_composite(img, light_layer)

    dark = Image.effect_noise(img.size, 28).convert("L")
    dark_alpha = dark.point(lambda p: max(0, min(18, int(abs(p - 128) * 0.22))))
    dark_layer = Image.new("RGBA", img.size, (0, 0, 0, 0))
    dark_layer.putalpha(dark_alpha)
    img = Image.alpha_composite(img, dark_layer)

    draw = ImageDraw.Draw(img, "RGBA")
    for _ in range(120):
        x = rng.randint(sc(38), sc(WIDTH - 38))
        y = rng.randint(sc(44), sc(HEIGHT - 44))
        a = rng.randint(10, 42)
        r = rng.choice([1, 1, 2, 2, 3])
        draw.ellipse((x - r, y - r, x + r, y + r), fill=(236, 218, 176, a))
    for _ in range(24):
        x = rng.randint(sc(64), sc(WIDTH - 64))
        y = rng.randint(sc(70), sc(HEIGHT - 70))
        length = rng.randint(sc(22), sc(94))
        draw.line((x, y, x + length, y + rng.randint(sc(-8), sc(8))), fill=(255, 255, 255, rng.randint(8, 18)), width=sc(1))
    return img


def vignette(img):
    width, height = img.size
    mask = Image.new("L", img.size, 0)
    draw = ImageDraw.Draw(mask)
    draw.ellipse((-sc(70), -sc(40), width + sc(70), height + sc(40)), fill=210)
    mask = mask.filter(ImageFilter.GaussianBlur(sc(70)))
    shade = Image.new("RGBA", img.size, (0, 0, 0, 168))
    shade.putalpha(ImageChops.invert(mask))
    return Image.alpha_composite(img, shade)


def glow_layer(size, shapes, blur):
    layer = Image.new("RGBA", size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(layer, "RGBA")
    for kind, payload, color in shapes:
        if kind == "ellipse":
            draw.ellipse(sbox(payload), fill=color)
        elif kind == "line":
            coords, width = payload
            draw.line(tuple(sc(v) for v in coords), fill=color, width=sc(width))
        elif kind == "polygon":
            draw.polygon(spoints(payload), fill=color)
    return layer.filter(ImageFilter.GaussianBlur(sc(blur)))


def draw_crack(draw, points, color=(8, 10, 12, 220), width=4, branch=True):
    scaled = spoints(points)
    draw.line(scaled, fill=color, width=sc(width), joint="curve")
    if branch:
        for i in range(1, len(points) - 1):
            x, y = points[i]
            angle = -0.9 if i % 2 == 0 else 0.85
            length = 38 - i * 3
            end = (x + math.cos(angle) * length, y + math.sin(angle) * length)
            draw.line((sc(x), sc(y), sc(end[0]), sc(end[1])), fill=rgba(color, max(80, color[3] - 58)), width=sc(max(1, width - 2)))


def draw_archive_background(draw, rng, accent):
    for i in range(8):
        x = 64 + i * 55 + rng.randint(-6, 6)
        draw.line((sc(x), sc(106), sc(x - 34), sc(654)), fill=(190, 196, 184, 14), width=sc(1))
    for y in [126, 178, 230, 586, 638]:
        draw.line((sc(58), sc(y), sc(454), sc(y + rng.randint(-5, 5))), fill=(255, 244, 210, 15), width=sc(2))

    for _ in range(18):
        x = rng.randint(72, 430)
        y = rng.randint(100, 630)
        if rng.random() < 0.55:
            draw.rectangle(sbox((x, y, x + rng.randint(14, 30), y + rng.randint(4, 8))), fill=(255, 220, 145, rng.randint(18, 50)))

    for _ in range(12):
        x = rng.randint(86, 420)
        y = rng.randint(100, 640)
        r = rng.randint(1, 3)
        draw.ellipse((sc(x - r), sc(y - r), sc(x + r), sc(y + r)), fill=rgba(accent, rng.randint(28, 76)))


def draw_card_frame(draw, rng, palette):
    accent = palette["accent"]
    edge = palette["edge"]
    inner = palette["inner"]

    draw.rounded_rectangle(sbox((18, 16, 494, 752)), radius=sc(32), fill=(17, 20, 23, 248), outline=edge, width=sc(5))
    draw.rounded_rectangle(sbox((30, 30, 482, 738)), radius=sc(26), fill=inner, outline=(245, 228, 188, 42), width=sc(2))
    draw.rounded_rectangle(sbox((52, 72, 460, 670)), radius=sc(18), fill=(0, 0, 0, 48), outline=(255, 238, 204, 30), width=sc(2))

    draw.polygon(
        spoints([(68, 618), (444, 592), (426, 660), (88, 682)]),
        fill=(16, 18, 19, 150),
        outline=(224, 215, 186, 34),
    )
    for i in range(7):
        y = 612 + i * 9
        draw.line((sc(88), sc(y), sc(426), sc(y - 22 + rng.randint(-2, 2))), fill=(242, 225, 184, 18), width=sc(1))

    for i in range(3):
        x = 76 + i * 22
        draw.ellipse(sbox((x, 704, x + 7, 711)), fill=rgba(accent, 145 - i * 28))
    for i in range(3):
        x = 429 - i * 22
        draw.rectangle(sbox((x, 704, x + 8, 712)), fill=(230, 220, 185, 54))

    draw.line((sc(70), sc(91), sc(442), sc(78)), fill=rgba(accent, 116), width=sc(3))
    draw.line((sc(72), sc(96), sc(390), sc(88)), fill=(255, 240, 188, 36), width=sc(1))


def draw_slab(draw, rng, y=470, broken=False):
    top = [(116, y - 48), (396, y - 70), (426, y + 8), (148, y + 34)]
    side = [(148, y + 34), (426, y + 8), (410, y + 52), (132, y + 82)]
    draw.polygon(spoints(side), fill=(38, 42, 42, 238), outline=(7, 9, 10, 105))
    draw.polygon(spoints(top), fill=(91, 96, 92, 238), outline=(231, 222, 194, 52))
    draw.line((sc(124), sc(y - 26), sc(410), sc(y - 48)), fill=(255, 241, 202, 30), width=sc(2))
    for _ in range(8):
        x = rng.randint(138, 390)
        yy = rng.randint(y - 52, y + 4)
        draw.line((sc(x), sc(yy), sc(x + rng.randint(-26, 30)), sc(yy + rng.randint(6, 24))), fill=(6, 8, 9, 80), width=sc(rng.randint(1, 3)))
    if broken:
        crack = [(260, y - 62), (238, y - 28), (266, y + 0), (244, y + 34)]
        draw_crack(draw, crack, (6, 7, 8, 230), 5)


def draw_card_base(seed, palette):
    rng = random.Random(seed)
    img = gradient((WIDTH * SCALE, HEIGHT * SCALE), palette["top"], palette["bottom"])

    glow = glow_layer(
        img.size,
        [
            ("ellipse", (132, 196, 382, 512), rgba(palette["accent"], 38)),
            ("line", ((86, 650, 430, 622), 14), rgba(palette["accent"], 34)),
        ],
        28,
    )
    img = Image.alpha_composite(img, glow)

    draw = ImageDraw.Draw(img, "RGBA")
    draw_archive_background(draw, rng, palette["accent"])
    draw_card_frame(draw, rng, palette)
    return img, draw, rng


def finish(img, path, seed):
    img = add_texture(img, seed)
    img = vignette(img)
    img = img.resize((WIDTH, HEIGHT), Image.Resampling.LANCZOS)
    img.save(path)


def draw_tap(draw, rng, palette):
    accent = palette["accent"]
    draw_slab(draw, rng, 472, False)
    glow = glow_layer(
        (WIDTH * SCALE, HEIGHT * SCALE),
        [("ellipse", (212, 270, 302, 360), rgba(accent, 58))],
        15,
    )
    draw.bitmap((0, 0), glow)
    draw.ellipse(sbox((218, 284, 294, 360)), outline=rgba(MEMORY_LIGHT, 190), width=sc(5))
    draw.ellipse(sbox((244, 316, 268, 340)), fill=rgba(accent, 185))
    draw.line((sc(256), sc(348), sc(256), sc(392)), fill=rgba(accent, 170), width=sc(4))
    draw_crack(draw, [(254, 400), (242, 427), (260, 450), (235, 484), (248, 512)], (8, 10, 11, 225), 3)
    for x, y in [(220, 438), (284, 430), (296, 478)]:
        draw.line((sc(x), sc(y), sc(x + rng.randint(-26, 26)), sc(y + rng.randint(14, 34))), fill=(8, 10, 11, 130), width=sc(1))


def draw_strike(draw, rng, palette):
    accent = palette["accent"]
    draw_slab(draw, rng, 478, True)
    impact = [(254, 268), (334, 412), (286, 408), (336, 574), (186, 414), (238, 418)]
    glow = glow_layer(
        (WIDTH * SCALE, HEIGHT * SCALE),
        [("polygon", impact, rgba(accent, 52)), ("ellipse", (150, 344, 366, 560), rgba(accent, 38))],
        22,
    )
    draw.bitmap((0, 0), glow)
    draw.polygon(spoints(impact), fill=rgba(accent, 214), outline=rgba(MEMORY_LIGHT, 118))
    for angle in range(0, 360, 38):
        length = rng.randint(54, 112)
        x2 = 256 + math.cos(math.radians(angle)) * length
        y2 = 438 + math.sin(math.radians(angle)) * length * 0.78
        draw.line((sc(256), sc(438), sc(x2), sc(y2)), fill=rgba(accent, rng.randint(48, 116)), width=sc(rng.randint(2, 5)))
    draw_crack(draw, [(254, 376), (215, 420), (242, 466), (188, 548)], (6, 7, 8, 235), 7)
    draw_crack(draw, [(266, 392), (318, 440), (338, 526)], (6, 7, 8, 210), 5)


def draw_stabilize(draw, rng, palette):
    accent = palette["accent"]
    draw_slab(draw, rng, 458, True)
    for x in [164, 306]:
        draw.rounded_rectangle(sbox((x, 306, x + 42, 574)), radius=sc(9), fill=rgba(SUPPORT_BLUE, 220), outline=(216, 229, 214, 84), width=sc(3))
        for y in [352, 402, 508]:
            draw.line((sc(x - 12), sc(y), sc(x + 54), sc(y - 18)), fill=rgba(PAPER, 190), width=sc(7))
            draw.line((sc(x - 10), sc(y + 6), sc(x + 52), sc(y - 12)), fill=(95, 70, 45, 74), width=sc(2))
    draw.rounded_rectangle(sbox((128, 410, 386, 462)), radius=sc(12), fill=rgba(PAPER, 205), outline=rgba(accent, 130), width=sc(3))
    draw.line((sc(138), sc(436), sc(378), sc(416)), fill=rgba(MEMORY_LIGHT, 80), width=sc(3))
    for x in [190, 256, 322]:
        draw.ellipse(sbox((x - 8, 428 - 8, x + 8, 428 + 8)), fill=rgba(accent, 170))


def draw_inspect_crack(draw, rng, palette):
    accent = palette["accent"]
    draw_slab(draw, rng, 488, True)
    lens_box = (145, 288, 335, 478)
    glow = glow_layer(
        (WIDTH * SCALE, HEIGHT * SCALE),
        [("ellipse", lens_box, rgba(MEMORY_LIGHT, 44)), ("line", ((315, 464, 402, 552), 22), rgba(accent, 32))],
        18,
    )
    draw.bitmap((0, 0), glow)
    draw.ellipse(sbox(lens_box), outline=rgba(MEMORY_LIGHT, 210), width=sc(10))
    draw.ellipse(sbox((164, 306, 316, 458)), fill=(210, 230, 220, 24), outline=(255, 255, 255, 36), width=sc(2))
    draw.line((sc(312), sc(462), sc(402), sc(552)), fill=rgba(accent, 210), width=sc(16))
    draw.line((sc(326), sc(476), sc(382), sc(532)), fill=(70, 48, 30, 98), width=sc(5))
    draw_crack(draw, [(238, 332), (258, 378), (238, 428), (262, 462)], (7, 8, 9, 230), 5)
    draw.arc(sbox((174, 318, 306, 450)), 210, 306, fill=(255, 255, 255, 120), width=sc(4))


def draw_cut_support(draw, rng, palette):
    accent = palette["accent"]
    draw_slab(draw, rng, 500, False)
    left = [(142, 330), (206, 316), (238, 568), (176, 582)]
    right = [(300, 320), (366, 338), (328, 586), (266, 568)]
    draw.polygon(spoints(left), fill=(116, 135, 139, 225), outline=(224, 236, 226, 72))
    draw.polygon(spoints(right), fill=(116, 135, 139, 225), outline=(224, 236, 226, 72))
    cut = [(204, 404), (294, 380), (314, 432), (218, 458)]
    glow = glow_layer((WIDTH * SCALE, HEIGHT * SCALE), [("polygon", cut, rgba(accent, 58))], 18)
    draw.bitmap((0, 0), glow)
    draw.polygon(spoints(cut), fill=rgba(accent, 218), outline=rgba(MEMORY_LIGHT, 104))
    draw.line((sc(130), sc(464), sc(388), sc(392)), fill=rgba(CORE_RED, 180), width=sc(9))
    draw.line((sc(152), sc(468), sc(356), sc(412)), fill=(9, 10, 11, 210), width=sc(5))
    for p in [(230, 408), (270, 434), (254, 456), (214, 424)]:
        draw.line((sc(254), sc(424), sc(p[0]), sc(p[1])), fill=(9, 10, 11, 130), width=sc(3))


def draw_chain_shock(draw, rng, palette):
    accent = palette["accent"]
    draw_slab(draw, rng, 506, True)
    for i, x in enumerate([150, 256, 362]):
        y = 410 + (i % 2) * 24
        draw.rounded_rectangle(sbox((x - 42, y - 35, x + 42, y + 35)), radius=sc(12), fill=(105, 114, 116, 210), outline=rgba(accent, 128), width=sc(4))
        draw.line((sc(x - 24), sc(y), sc(x + 24), sc(y)), fill=(8, 10, 11, 132), width=sc(4))
        draw.line((sc(x + 42), sc(y), sc(x + 64), sc(y + 13)), fill=rgba(accent, 118), width=sc(5))
    for r, a in [(60, 116), (96, 86), (134, 58), (176, 34)]:
        draw.ellipse(sbox((256 - r, 428 - r * 0.72, 256 + r, 428 + r * 0.72)), outline=rgba(accent, a), width=sc(5))
    for x in [184, 256, 328]:
        draw.line((sc(x), sc(354), sc(x + rng.randint(-18, 18)), sc(542)), fill=rgba(MEMORY_LIGHT, 48), width=sc(2))


def draw_sealed_echo(draw, rng, palette):
    accent = palette["accent"]
    draw_slab(draw, rng, 506, False)
    cx, cy = 256, 424
    glow = glow_layer(
        (WIDTH * SCALE, HEIGHT * SCALE),
        [("ellipse", (126, 292, 386, 558), rgba(accent, 72))],
        26,
    )
    draw.bitmap((0, 0), glow)
    for i in range(4):
        inset = i * 26
        alpha = 220 - i * 42
        draw.arc(sbox((130 + inset, 300 + inset, 382 - inset, 552 - inset)), 35, 318, fill=rgba(accent, alpha), width=sc(8 - i))
    for angle in [35, 140, 250]:
        x = cx + math.cos(math.radians(angle)) * 122
        y = cy + math.sin(math.radians(angle)) * 96
        draw.ellipse(sbox((x - 8, y - 8, x + 8, y + 8)), fill=rgba(MEMORY_LIGHT, 205))
    points = []
    for i in range(12):
        angle = -math.pi / 2 + i * math.pi / 6
        radius = 62 if i % 2 == 0 else 28
        points.append((cx + math.cos(angle) * radius, cy + math.sin(angle) * radius))
    draw.polygon(spoints(points), fill=rgba(MEMORY_LIGHT, 218), outline=rgba(PAPER, 180))
    draw.ellipse(sbox((224, 392, 288, 456)), outline=rgba(ARCHIVE_BLACK, 124), width=sc(4))


def draw_core_fracture(draw, rng, palette):
    accent = palette["accent"]
    draw_slab(draw, rng, 520, True)
    core_box = (150, 294, 362, 510)
    glow = glow_layer(
        (WIDTH * SCALE, HEIGHT * SCALE),
        [("ellipse", (116, 258, 398, 548), rgba(CORE_RED, 86)), ("ellipse", core_box, rgba(MEMORY_GOLD, 30))],
        30,
    )
    draw.bitmap((0, 0), glow)
    draw.ellipse(sbox(core_box), fill=rgba(CORE_DARK, 240), outline=rgba(accent, 230), width=sc(10))
    shard_a = [(256, 306), (318, 418), (270, 508), (202, 430)]
    shard_b = [(246, 318), (224, 408), (248, 492), (190, 438)]
    draw.polygon(spoints(shard_a), fill=rgba(CORE_RED, 228), outline=rgba(MEMORY_LIGHT, 88))
    draw.polygon(spoints(shard_b), fill=(145, 32, 34, 235), outline=(255, 180, 114, 64))
    draw_crack(draw, [(258, 314), (244, 382), (268, 428), (242, 504)], (12, 8, 9, 238), 7)
    draw_crack(draw, [(270, 426), (324, 456), (350, 508)], (12, 8, 9, 225), 5)
    for r, a in [(60, 112), (92, 62), (124, 34)]:
        draw.ellipse(sbox((256 - r, 406 - r, 256 + r, 406 + r)), outline=rgba(CORE_RED, a), width=sc(5))


CARDS = {
    "tap": {
        "seed": 311,
        "accent": (232, 190, 104, 255),
        "top": (37, 42, 43, 255),
        "bottom": (13, 15, 16, 255),
        "edge": (224, 210, 174, 104),
        "inner": (37, 42, 43, 242),
        "draw": draw_tap,
    },
    "strike": {
        "seed": 419,
        "accent": (232, 125, 66, 255),
        "top": (42, 41, 39, 255),
        "bottom": (14, 13, 13, 255),
        "edge": (234, 160, 88, 118),
        "inner": (43, 40, 37, 244),
        "draw": draw_strike,
    },
    "stabilize": {
        "seed": 523,
        "accent": (158, 207, 184, 255),
        "top": (31, 45, 46, 255),
        "bottom": (10, 14, 15, 255),
        "edge": (165, 210, 195, 114),
        "inner": (30, 45, 46, 244),
        "draw": draw_stabilize,
    },
    "inspect_crack": {
        "seed": 631,
        "accent": (188, 205, 204, 255),
        "top": (31, 39, 44, 255),
        "bottom": (10, 13, 16, 255),
        "edge": (198, 214, 212, 104),
        "inner": (31, 39, 44, 244),
        "draw": draw_inspect_crack,
    },
    "cut_support": {
        "seed": 743,
        "accent": (232, 152, 76, 255),
        "top": (31, 44, 51, 255),
        "bottom": (9, 12, 15, 255),
        "edge": (144, 180, 196, 116),
        "inner": (28, 42, 50, 244),
        "draw": draw_cut_support,
    },
    "chain_shock": {
        "seed": 857,
        "accent": (132, 169, 199, 255),
        "top": (28, 40, 53, 255),
        "bottom": (8, 11, 16, 255),
        "edge": (145, 176, 205, 118),
        "inner": (27, 39, 53, 244),
        "draw": draw_chain_shock,
    },
    "sealed_echo": {
        "seed": 967,
        "accent": (239, 185, 72, 255),
        "top": (41, 36, 45, 255),
        "bottom": (13, 11, 16, 255),
        "edge": (239, 190, 88, 136),
        "inner": (39, 35, 44, 244),
        "draw": draw_sealed_echo,
    },
    "core_fracture": {
        "seed": 1087,
        "accent": (236, 84, 67, 255),
        "top": (55, 29, 31, 255),
        "bottom": (16, 8, 9, 255),
        "edge": (240, 118, 82, 156),
        "inner": (55, 28, 30, 246),
        "draw": draw_core_fracture,
    },
}


def make_card(card_id, spec):
    img, draw, rng = draw_card_base(spec["seed"], spec)
    spec["draw"](draw, rng, spec)
    finish(img, OUT / f"{card_id}.png", spec["seed"])


def main():
    OUT.mkdir(parents=True, exist_ok=True)
    for card_id, spec in CARDS.items():
        make_card(card_id, spec)
    print(f"Generated {len(CARDS)} Agent card illustrations in {OUT}")


if __name__ == "__main__":
    main()
