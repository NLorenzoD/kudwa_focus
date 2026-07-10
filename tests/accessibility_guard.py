#!/usr/bin/env python3
"""Prevent the previously shipped high-frequency flashing behaviour from returning."""

from pathlib import Path
import sys


project_root = Path(__file__).resolve().parents[1]
stage_source = (project_root / "src" / "kudwa_focus" / "stage_panel.cs").read_text(encoding="utf-8")
form_source = (project_root / "src" / "kudwa_focus" / "main_form.cs").read_text(encoding="utf-8")

forbidden = {
    "stage_panel.cs": (
        "animation_phase",
        "pulse_pen",
        "draw_motion_lines",
    ),
    "main_form.cs": (
        "Interval = 50",
        "animation_speed",
        "animation_phase",
    ),
}

failures: list[str] = []

for file_name, patterns in forbidden.items():
    source = stage_source if file_name == "stage_panel.cs" else form_source

    for pattern in patterns:
        if pattern in source:
            failures.append(f"{file_name} contains forbidden flashing pattern: {pattern}")

required = (
    ("main_form.cs", form_source, "Interval = 100"),
    ("main_form.cs", form_source, "displayed_second != last_displayed_second"),
    ("stage_panel.cs", stage_source, "draw_structure_lines"),
)

for file_name, source, pattern in required:
    if pattern not in source:
        failures.append(f"{file_name} is missing reduced-motion safeguard: {pattern}")

if failures:
    print("Accessibility guard failed:", file=sys.stderr)

    for failure in failures:
        print(f"- {failure}", file=sys.stderr)

    raise SystemExit(1)

print("Accessibility guard passed: no pulse, moving background, or 50 ms full-stage repaint loop.")
