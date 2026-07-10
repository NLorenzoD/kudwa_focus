using kudwa_focus;

var failures = new List<string>();

void check(string name, bool condition)
{
    if (!condition)
    {
        failures.Add(name);
    }
}

check("five default presets exist", timer_catalog.presets.Count == 5);
check("break is ten minutes", timer_catalog.presets[0] == new timer_preset("Break", TimeSpan.FromMinutes(10)));
check("tea break is fifteen minutes", timer_catalog.presets[1] == new timer_preset("Tea Break", TimeSpan.FromMinutes(15)));
check("lunch break is one hour", timer_catalog.presets[2] == new timer_preset("Lunch Break", TimeSpan.FromHours(1)));
check("exercise is five minutes", timer_catalog.presets[3] == new timer_preset("Exercise", TimeSpan.FromMinutes(5)));
check("quick exercise is three minutes", timer_catalog.presets[4] == new timer_preset("Quick Exercise", TimeSpan.FromMinutes(3)));
check("ten minutes formats correctly", timer_math.format_remaining(TimeSpan.FromMinutes(10)) == "10:00");
check("one hour formats correctly", timer_math.format_remaining(TimeSpan.FromHours(1)) == "1:00:00");
check("display rounds remaining time upward", timer_math.format_remaining(TimeSpan.FromSeconds(9.1)) == "00:10");
check("normal phase is calm", timer_math.phase_for(TimeSpan.FromSeconds(61), false) == audio_phase.calm);
check("sixty seconds starts final minute", timer_math.phase_for(TimeSpan.FromSeconds(60), false) == audio_phase.final_minute);
check("thirty seconds starts halfway through crescendo", timer_math.final_track_offset_milliseconds(TimeSpan.FromSeconds(30)) == 30_000);
check("progress is clamped", timer_math.progress(TimeSpan.FromMinutes(10), TimeSpan.FromMinutes(-1)) == 1.0);
check("completed timer is silent", timer_math.phase_for(TimeSpan.Zero, true) == audio_phase.silent);

if (failures.Count > 0)
{
    Console.Error.WriteLine("KUDWA Focus checks failed:");

    foreach (var failure in failures)
    {
        Console.Error.WriteLine($"- {failure}");
    }

    return 1;
}

Console.WriteLine("All 14 KUDWA Focus checks passed.");
return 0;
