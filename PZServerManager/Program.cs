using PZServerManager.Forms;
using PZServerManager.Models;
using PZServerManager.Services;

namespace PZServerManager;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        var config = AppConfigStore.Load();
        if (!config.IsBootstrapped)
        {
            using var setup = new FirstRunForm();
            if (setup.ShowDialog() != DialogResult.OK || setup.Result is null)
                return;
            config = setup.Result;
        }

        Application.Run(new MainForm(config));
    }
}
