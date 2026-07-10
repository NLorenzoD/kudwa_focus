namespace kudwa_focus;

public enum audio_phase
{
    calm,
    final_minute,
    silent
}

public static class timer_math
{
    public static string format_remaining(TimeSpan remaining)
    {
        if (remaining <= TimeSpan.Zero)
        {
            return "00:00";
        }

        var total_seconds = (long)Math.Ceiling(remaining.TotalSeconds);
        var hours = total_seconds / 3_600;
        var minutes = total_seconds % 3_600 / 60;
        var seconds = total_seconds % 60;

        return hours > 0
            ? $"{hours}:{minutes:00}:{seconds:00}"
            : $"{minutes:00}:{seconds:00}";
    }

    public static double progress(TimeSpan total, TimeSpan remaining)
    {
        if (total <= TimeSpan.Zero)
        {
            return 0.0;
        }

        return Math.Clamp(1.0 - remaining.TotalMilliseconds / total.TotalMilliseconds, 0.0, 1.0);
    }

    public static audio_phase phase_for(TimeSpan remaining, bool is_complete)
    {
        if (is_complete || remaining <= TimeSpan.Zero)
        {
            return audio_phase.silent;
        }

        return remaining <= TimeSpan.FromSeconds(60)
            ? audio_phase.final_minute
            : audio_phase.calm;
    }

    public static int final_track_offset_milliseconds(TimeSpan remaining)
    {
        var offset = TimeSpan.FromSeconds(60) - remaining;
        return (int)Math.Clamp(offset.TotalMilliseconds, 0.0, 59_950.0);
    }
}
