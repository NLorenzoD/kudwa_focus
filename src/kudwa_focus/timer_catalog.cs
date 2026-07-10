namespace kudwa_focus;

public static class timer_catalog
{
    public static IReadOnlyList<timer_preset> presets { get; } =
    [
        new("Break", TimeSpan.FromMinutes(10)),
        new("Tea Break", TimeSpan.FromMinutes(15)),
        new("Lunch Break", TimeSpan.FromMinutes(60)),
        new("Exercise", TimeSpan.FromMinutes(5)),
        new("Quick Exercise", TimeSpan.FromMinutes(3))
    ];
}
