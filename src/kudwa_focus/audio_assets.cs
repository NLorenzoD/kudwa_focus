using System.Reflection;

namespace kudwa_focus;

internal sealed record audio_asset_paths(string calm_loop, string final_minute);

internal static class audio_assets
{
    private const string resource_prefix = "kudwa_focus.assets.audio";

    public static audio_asset_paths prepare()
    {
        var output_directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "kudwa_focus",
            "audio",
            "v1");
        Directory.CreateDirectory(output_directory);

        return new audio_asset_paths(
            extract("gentle_break_loop.wav", output_directory),
            extract("final_minute_crescendo.wav", output_directory));
    }

    private static string extract(string file_name, string output_directory)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resource_name = $"{resource_prefix}.{file_name}";
        using var resource_stream = assembly.GetManifestResourceStream(resource_name)
            ?? throw new InvalidOperationException($"Embedded audio resource not found: {resource_name}");

        var output_path = Path.Combine(output_directory, file_name);
        var should_write = !File.Exists(output_path)
            || new FileInfo(output_path).Length != resource_stream.Length;

        if (should_write)
        {
            using var output_stream = new FileStream(output_path, FileMode.Create, FileAccess.Write, FileShare.None);
            resource_stream.CopyTo(output_stream);
        }

        return output_path;
    }
}
