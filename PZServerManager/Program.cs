using PZServerManager.Services;

namespace PZServerManager;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        // Surface any startup or UI-thread exceptions instead of letting the process die silently.
        // Without this, a constructor throw on MainForm just dies — exactly the symptom we hit before.
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (_, e) => Show("UI thread exception", e.Exception);
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            if (e.ExceptionObject is Exception ex) Show("Unhandled exception", ex);
        };

        ApplicationConfiguration.Initialize();

        try
        {
            var config = AppConfigStore.Load();
            Application.Run(new MainForm(config));
        }
        catch (Exception ex)
        {
            Show("Failed to start", ex);
        }
    }

    private static void Show(string title, Exception ex)
        => MessageBox.Show($"{ex.GetType().Name}: {ex.Message}\n\n{ex}",
            $"PZ Server Manager — {title}",
            MessageBoxButtons.OK, MessageBoxIcon.Error);
}
