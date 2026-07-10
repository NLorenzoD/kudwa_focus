#!/usr/bin/env python3
"""Generate the deterministic KUDWA Focus SVG, PNG and multi-size Windows icon."""

from __future__ import annotations

import hashlib
import json
from pathlib import Path
import struct
import zlib


project_root = Path(__file__).resolve().parents[1]
output_directory = project_root / "src" / "kudwa_focus" / "assets" / "icon"
icon_sizes = (16, 24, 32, 48, 64, 128, 256)

deep = (14, 17, 16, 255)
surface = (28, 34, 31, 255)
ivory = (246, 241, 229, 255)
orange = (255, 91, 31, 255)
amber = (255, 184, 0, 255)
red = (255, 45, 45, 255)
transparent = (0, 0, 0, 0)

upper_arm = ((88, 118), (158, 48), (210, 48), (130, 130))
lower_arm = ((88, 136), (132, 126), (210, 208), (158, 208))
centre_diamond = ((111, 128), (128, 111), (145, 128), (128, 145))


def inside_rounded_square(x: float, y: float) -> bool:
    radius = 42.0

    if radius <= x <= 256.0 - radius or radius <= y <= 256.0 - radius:
        return 0.0 <= x <= 256.0 and 0.0 <= y <= 256.0

    corner_x = radius if x < radius else 256.0 - radius
    corner_y = radius if y < radius else 256.0 - radius
    return (x - corner_x) ** 2 + (y - corner_y) ** 2 <= radius ** 2


def inside_polygon(x: float, y: float, points: tuple[tuple[int, int], ...]) -> bool:
    inside = False
    previous = points[-1]

    for current in points:
        x1, y1 = previous
        x2, y2 = current

        if (y1 > y) != (y2 > y):
            intersection_x = (x2 - x1) * (y - y1) / (y2 - y1) + x1

            if x < intersection_x:
                inside = not inside

        previous = current

    return inside


def blend(base: tuple[int, int, int, int], overlay: tuple[int, int, int, int], opacity: float) -> tuple[int, int, int, int]:
    return (
        round(base[0] * (1.0 - opacity) + overlay[0] * opacity),
        round(base[1] * (1.0 - opacity) + overlay[1] * opacity),
        round(base[2] * (1.0 - opacity) + overlay[2] * opacity),
        255,
    )


def colour_at(x: float, y: float) -> tuple[int, int, int, int]:
    if not inside_rounded_square(x, y):
        return transparent

    colour = deep

    if ((x + y + 18.0) % 54.0) < 5.0:
        colour = blend(colour, orange, 0.11)

    if 20.0 <= x <= 236.0 and 20.0 <= y <= 236.0:
        border_distance = min(x - 20.0, 236.0 - x, y - 20.0, 236.0 - y)

        if border_distance < 2.5:
            colour = surface

    if 52.0 <= x <= 91.0 and 48.0 <= y <= 208.0:
        colour = ivory

    if inside_polygon(x, y, upper_arm):
        colour = orange

    if inside_polygon(x, y, lower_arm):
        colour = amber

    if inside_polygon(x, y, centre_diamond):
        colour = red

    return colour


def render_rgba(size: int) -> bytes:
    supersampling = 4
    output = bytearray()

    for pixel_y in range(size):
        for pixel_x in range(size):
            totals = [0, 0, 0, 0]

            for sample_y in range(supersampling):
                for sample_x in range(supersampling):
                    x = (pixel_x + (sample_x + 0.5) / supersampling) * 256.0 / size
                    y = (pixel_y + (sample_y + 0.5) / supersampling) * 256.0 / size
                    colour = colour_at(x, y)

                    for channel in range(4):
                        totals[channel] += colour[channel]

            sample_count = supersampling * supersampling
            output.extend(round(total / sample_count) for total in totals)

    return bytes(output)


def png_chunk(kind: bytes, data: bytes) -> bytes:
    return struct.pack(">I", len(data)) + kind + data + struct.pack(">I", zlib.crc32(kind + data) & 0xFFFFFFFF)


def encode_png(size: int, rgba: bytes) -> bytes:
    stride = size * 4
    scanlines = b"".join(b"\x00" + rgba[row * stride:(row + 1) * stride] for row in range(size))
    header = struct.pack(">IIBBBBB", size, size, 8, 6, 0, 0, 0)
    return b"\x89PNG\r\n\x1a\n" + png_chunk(b"IHDR", header) + png_chunk(b"IDAT", zlib.compress(scanlines, 9)) + png_chunk(b"IEND", b"")


def write_ico(path: Path, png_images: list[tuple[int, bytes]]) -> None:
    header = struct.pack("<HHH", 0, 1, len(png_images))
    entries = bytearray()
    payload = bytearray()
    offset = 6 + 16 * len(png_images)

    for size, png_data in png_images:
        dimension = 0 if size == 256 else size
        entries.extend(struct.pack("<BBBBHHII", dimension, dimension, 0, 0, 1, 32, len(png_data), offset))
        payload.extend(png_data)
        offset += len(png_data)

    path.write_bytes(header + entries + payload)


def write_svg(path: Path) -> None:
    path.write_text(
        """<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 256 256" role="img" aria-label="KUDWA Focus geometric K icon">
  <defs>
    <clipPath id="icon-shape"><rect width="256" height="256" rx="42"/></clipPath>
    <pattern id="diagonals" width="54" height="54" patternUnits="userSpaceOnUse" patternTransform="rotate(-45)">
      <rect width="5" height="54" fill="#ff5b1f" opacity="0.11"/>
    </pattern>
  </defs>
  <g clip-path="url(#icon-shape)">
    <rect width="256" height="256" fill="#0e1110"/>
    <rect width="256" height="256" fill="url(#diagonals)"/>
    <rect x="21.25" y="21.25" width="213.5" height="213.5" fill="none" stroke="#1c221f" stroke-width="2.5"/>
    <path d="M52 48H91V208H52Z" fill="#f6f1e5"/>
    <path d="M88 118L158 48H210L130 130Z" fill="#ff5b1f"/>
    <path d="M88 136L132 126L210 208H158Z" fill="#ffb800"/>
    <path d="M111 128L128 111L145 128L128 145Z" fill="#ff2d2d"/>
  </g>
</svg>
""",
        encoding="utf-8",
    )


def digest(path: Path) -> str:
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main() -> None:
    output_directory.mkdir(parents=True, exist_ok=True)
    svg_path = output_directory / "kudwa_focus_icon.svg"
    png_path = output_directory / "kudwa_focus_icon.png"
    ico_path = output_directory / "kudwa_focus_icon.ico"
    write_svg(svg_path)

    png_images: list[tuple[int, bytes]] = []

    for size in icon_sizes:
        png_images.append((size, encode_png(size, render_rgba(size))))

    png_path.write_bytes(png_images[-1][1])
    write_ico(ico_path, png_images)

    manifest = {
        "title": "KUDWA Focus application icon",
        "design": "Geometric K mark using the app's angular KUDWA-inspired visual language.",
        "source": "tools/generate_icon.py",
        "files": {
            svg_path.name: digest(svg_path),
            png_path.name: digest(png_path),
            ico_path.name: digest(ico_path),
        },
        "ico_sizes": list(icon_sizes),
    }
    manifest_path = output_directory / "icon_manifest.json"
    manifest_path.write_text(json.dumps(manifest, indent=2) + "\n", encoding="utf-8")

    print(f"generated {svg_path.relative_to(project_root)}")
    print(f"generated {png_path.relative_to(project_root)}")
    print(f"generated {ico_path.relative_to(project_root)}")


if __name__ == "__main__":
    main()
