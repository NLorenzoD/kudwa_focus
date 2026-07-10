namespace kudwa_focus;

internal static class kudwa_focus_program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new main_form());
    }
}
