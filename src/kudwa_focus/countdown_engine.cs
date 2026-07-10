using System.Diagnostics;

namespace kudwa_focus;

public sealed class countdown_engine
{
    private readonly Stopwatch stopwatch = new();
    private TimeSpan stored_remaining;

    public string activity_name { get; private set; } = string.Empty;
    public TimeSpan total_duration { get; private set; }
    public bool is_running => stopwatch.IsRunning;
    public bool is_complete => remaining <= TimeSpan.Zero;

    public TimeSpan remaining
    {
        get
        {
            if (!stopwatch.IsRunning)
            {
                return stored_remaining;
            }

            var current = stored_remaining - stopwatch.Elapsed;
            return current > TimeSpan.Zero ? current : TimeSpan.Zero;
        }
    }

    public void select(string name, TimeSpan duration)
    {
        stopwatch.Reset();
        activity_name = name;
        total_duration = duration > TimeSpan.Zero ? duration : TimeSpan.FromSeconds(1);
        stored_remaining = total_duration;
    }

    public void start()
    {
        if (stored_remaining <= TimeSpan.Zero)
        {
            stored_remaining = total_duration;
        }

        if (!stopwatch.IsRunning)
        {
            stopwatch.Restart();
        }
    }

    public void pause()
    {
        if (!stopwatch.IsRunning)
        {
            return;
        }

        stored_remaining = remaining;
        stopwatch.Reset();
    }

    public void reset()
    {
        stopwatch.Reset();
        stored_remaining = total_duration;
    }

    public void complete()
    {
        stopwatch.Reset();
        stored_remaining = TimeSpan.Zero;
    }

    public void adjust(TimeSpan change)
    {
        var was_running = stopwatch.IsRunning;
        var adjusted_remaining = remaining + change;
        stored_remaining = adjusted_remaining > TimeSpan.FromSeconds(1)
            ? adjusted_remaining
            : TimeSpan.FromSeconds(1);
        total_duration = total_duration + change > TimeSpan.FromSeconds(1)
            ? total_duration + change
            : TimeSpan.FromSeconds(1);
        total_duration = total_duration < stored_remaining ? stored_remaining : total_duration;
        stopwatch.Reset();

        if (was_running)
        {
            stopwatch.Start();
        }
    }
}
