using System.Runtime.InteropServices;
using System.Text;

namespace kudwa_focus;

internal sealed class audio_controller : IDisposable
{
    private const string audio_alias = "kudwa_focus_audio";
    private readonly audio_asset_paths? asset_paths;
    private bool disposed;
    private int volume = 720;

    public audio_phase current_phase { get; private set; } = audio_phase.silent;
    public bool is_available { get; }
    public string last_error { get; private set; } = string.Empty;

    public audio_controller()
    {
        try
        {
            asset_paths = audio_assets.prepare();
            is_available = true;
        }
        catch (Exception exception)
        {
            is_available = false;
            last_error = exception.Message;
        }
    }

    public void play_for(TimeSpan remaining, int requested_volume, bool force_restart = false)
    {
        if (!is_available || disposed || remaining <= TimeSpan.Zero)
        {
            return;
        }

        volume = Math.Clamp(requested_volume, 0, 1_000);
        var requested_phase = timer_math.phase_for(remaining, false);

        if (!force_restart && requested_phase == current_phase)
        {
            resume();
            set_volume(volume);
            return;
        }

        close_current();

        if (requested_phase == audio_phase.calm)
        {
            if (open(asset_paths!.calm_loop))
            {
                current_phase = audio_phase.calm;
                set_volume(volume);
                send($"play {audio_alias} repeat");
            }

            return;
        }

        if (open(asset_paths!.final_minute))
        {
            current_phase = audio_phase.final_minute;
            set_volume(volume);
            var offset = timer_math.final_track_offset_milliseconds(remaining);
            send($"play {audio_alias} from {offset}");
        }
    }

    public void pause()
    {
        if (current_phase != audio_phase.silent)
        {
            send($"pause {audio_alias}");
        }
    }

    public void resume()
    {
        if (current_phase != audio_phase.silent)
        {
            send($"resume {audio_alias}");
        }
    }

    public void set_volume(int requested_volume)
    {
        volume = Math.Clamp(requested_volume, 0, 1_000);

        if (current_phase != audio_phase.silent)
        {
            send($"setaudio {audio_alias} volume to {volume}");
        }
    }

    public void stop()
    {
        close_current();
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        close_current();
        disposed = true;
    }

    private bool open(string path)
    {
        var result = send($"open \"{path}\" type waveaudio alias {audio_alias}");

        if (result != 0)
        {
            current_phase = audio_phase.silent;
            return false;
        }

        send($"set {audio_alias} time format milliseconds");
        return true;
    }

    private void close_current()
    {
        if (current_phase == audio_phase.silent)
        {
            return;
        }

        send($"stop {audio_alias}");
        send($"close {audio_alias}");
        current_phase = audio_phase.silent;
    }

    private int send(string command)
    {
        var result = mci_send_string(command, null, 0, IntPtr.Zero);

        if (result != 0)
        {
            var message = new StringBuilder(256);
            last_error = mci_get_error_string(result, message, message.Capacity)
                ? message.ToString()
                : $"Windows audio error {result}";
        }

        return result;
    }

    [DllImport("winmm.dll", EntryPoint = "mciSendStringW", CharSet = CharSet.Unicode)]
    private static extern int mci_send_string(
        string command,
        StringBuilder? return_value,
        int return_length,
        IntPtr callback);

    [DllImport("winmm.dll", EntryPoint = "mciGetErrorStringW", CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool mci_get_error_string(
        int error_code,
        StringBuilder error_text,
        int error_text_size);
}
