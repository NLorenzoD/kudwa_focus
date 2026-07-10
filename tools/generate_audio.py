#!/usr/bin/env python3
"""Generate the original KUDWA Focus score as deterministic PCM WAV files."""

from __future__ import annotations

from array import array
import hashlib
import json
import math
from pathlib import Path
import wave


sample_rate = 22_050
maximum_sample = 32_767
project_root = Path(__file__).resolve().parents[1]
output_directory = project_root / "src" / "kudwa_focus" / "assets" / "audio"


def smooth_step(value: float) -> float:
    value = max(0.0, min(1.0, value))
    return value * value * (3.0 - 2.0 * value)


def soft_clip(value: float) -> float:
    return math.tanh(value)


def chord_value(chord: tuple[float, ...], time: float) -> float:
    return sum(
        math.sin(math.tau * frequency * time)
        + 0.22 * math.sin(math.tau * frequency * 2.0 * time)
        for frequency in chord
    ) / (len(chord) * 1.22)


def write_wave(path: Path, samples: array) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)

    with wave.open(str(path), "wb") as output:
        output.setnchannels(1)
        output.setsampwidth(2)
        output.setframerate(sample_rate)
        output.writeframes(samples.tobytes())


def render_calm_loop() -> tuple[array, float]:
    duration = 24.0
    chord_length = 6.0
    beat_length = 60.0 / 90.0
    chords = (
        (196.00, 246.94, 293.66),
        (164.81, 196.00, 246.94),
        (130.81, 164.81, 196.00),
        (146.83, 185.00, 220.00),
    )
    arpeggio = (392.00, 493.88, 587.33, 493.88, 329.63, 392.00)
    samples = array("h")
    total_samples = int(duration * sample_rate)

    for index in range(total_samples):
        time = index / sample_rate
        chord_position = time / chord_length
        chord_index = int(chord_position) % len(chords)
        next_chord_index = (chord_index + 1) % len(chords)
        within_chord = time % chord_length
        blend = smooth_step((within_chord - (chord_length - 1.25)) / 1.25)
        pad = (
            chord_value(chords[chord_index], time) * (1.0 - blend)
            + chord_value(chords[next_chord_index], time) * blend
        )

        beat_index = int(time / beat_length)
        beat_time = time % beat_length
        pluck_envelope = math.exp(-5.5 * beat_time)
        pluck_frequency = arpeggio[beat_index % len(arpeggio)]
        pluck = math.sin(math.tau * pluck_frequency * beat_time) * pluck_envelope
        pulse = math.sin(math.tau * 55.0 * beat_time) * math.exp(-11.0 * beat_time)
        shimmer = math.sin(math.tau * 1_760.0 * time) * (0.5 + 0.5 * math.sin(math.tau * time / 8.0))

        loop_fade = min(1.0, time / 0.035, (duration - time) / 0.035)
        value = (0.23 * pad + 0.095 * pluck + 0.04 * pulse + 0.012 * shimmer) * loop_fade
        samples.append(int(soft_clip(value) * maximum_sample))

    return samples, duration


def render_final_minute() -> tuple[array, float]:
    duration = 60.0
    ramp_duration = 50.0
    start_bpm = 104.0
    end_bpm = 184.0
    chords = (
        (196.00, 246.94, 293.66),
        (220.00, 261.63, 329.63),
        (246.94, 293.66, 369.99),
        (261.63, 329.63, 392.00),
    )
    arpeggio = (392.00, 493.88, 587.33, 659.25, 783.99, 659.25, 587.33, 493.88)
    samples = array("h")
    total_samples = int(duration * sample_rate)

    for index in range(total_samples):
        time = index / sample_rate
        ramp_time = min(time, ramp_duration)
        beats = (
            start_bpm * ramp_time / 60.0
            + 0.5 * (end_bpm - start_bpm) * ramp_time * ramp_time / (60.0 * ramp_duration)
        )

        if time > ramp_duration:
            beats += end_bpm * (time - ramp_duration) / 60.0

        beat_index = int(beats)
        beat_phase = beats - beat_index
        eighth_index = int(beats * 2.0)
        eighth_phase = beats * 2.0 - eighth_index
        energy = 0.22 + 0.54 * smooth_step(time / duration)

        chord_index = int(time / 5.0) % len(chords)
        pad = chord_value(chords[chord_index], time)
        arpeggio_frequency = arpeggio[eighth_index % len(arpeggio)]
        arpeggio_note = (
            math.sin(math.tau * arpeggio_frequency * time)
            * math.exp(-4.5 * eighth_phase)
        )
        kick = (
            math.sin(math.tau * (58.0 - 18.0 * beat_phase) * beat_phase)
            * math.exp(-12.0 * beat_phase)
        )
        high_pulse = (
            math.sin(math.tau * 4_100.0 * time)
            * math.exp(-25.0 * eighth_phase)
        )

        value = energy * (0.29 * pad + 0.22 * arpeggio_note + 0.26 * kick + 0.035 * high_pulse)

        if time >= 50.0:
            countdown_second = min(9, int(time - 50.0))
            countdown_phase = (time - 50.0) % 1.0
            impact_frequency = 170.0 + countdown_second * 22.0
            impact_envelope = math.exp(-5.8 * countdown_phase)
            impact = (
                0.62 * math.sin(math.tau * impact_frequency * countdown_phase)
                + 0.38 * math.sin(math.tau * impact_frequency * 0.5 * countdown_phase)
            ) * impact_envelope
            value += (0.36 + 0.025 * countdown_second) * impact

        if time >= 59.42:
            finale_time = time - 59.42
            finale_envelope = math.exp(-3.8 * finale_time)
            boom = (
                0.62 * math.sin(math.tau * 44.0 * finale_time)
                + 0.38 * math.sin(math.tau * 66.0 * finale_time)
            ) * finale_envelope
            cinematic_noise = (
                math.sin(math.tau * 1_337.0 * time)
                * math.sin(math.tau * 2_111.0 * time)
                * finale_envelope
            )
            value += 0.95 * boom + 0.28 * cinematic_noise

        end_fade = min(1.0, (duration - time) / 0.025)
        samples.append(int(soft_clip(value * 1.22) * end_fade * maximum_sample))

    return samples, duration


def file_record(path: Path, purpose: str, duration: float) -> dict[str, object]:
    digest = hashlib.sha256(path.read_bytes()).hexdigest()
    return {
        "file": path.name,
        "purpose": purpose,
        "duration_seconds": duration,
        "sample_rate": sample_rate,
        "channels": 1,
        "format": "pcm_s16le_wav",
        "sha256": digest,
        "provenance": "original_procedural_score_generated_by_tools/generate_audio.py",
    }


def main() -> None:
    calm_samples, calm_duration = render_calm_loop()
    final_samples, final_duration = render_final_minute()

    calm_path = output_directory / "gentle_break_loop.wav"
    final_path = output_directory / "final_minute_crescendo.wav"
    write_wave(calm_path, calm_samples)
    write_wave(final_path, final_samples)

    manifest = {
        "title": "KUDWA Focus original score",
        "licensing": "Original generated audio; safe to redistribute with this project.",
        "assets": [
            file_record(calm_path, "continuous gentle break-period loop", calm_duration),
            file_record(final_path, "60-second tempo crescendo with cinematic final ten", final_duration),
        ],
    }
    manifest_path = output_directory / "audio_manifest.json"
    manifest_path.write_text(json.dumps(manifest, indent=2) + "\n", encoding="utf-8")

    print(f"generated {calm_path.relative_to(project_root)} ({calm_duration:.0f}s)")
    print(f"generated {final_path.relative_to(project_root)} ({final_duration:.0f}s)")
    print(f"wrote {manifest_path.relative_to(project_root)}")


if __name__ == "__main__":
    main()
