using PZServerManager.Forms;
using PZServerManager.Services;

namespace PZServerManager;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        // The wizard is embedded in the "초기 설정" tab inside MainForm — no separate dialog.
        // If config is empty/not bootstrapped, the wizard cards show inside that tab.
        var config = AppConfigStore.Load();
        Application.Run(new MainForm(config));
    }
}
