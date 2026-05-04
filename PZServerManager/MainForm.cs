using PZServerManager.Models;

namespace PZServerManager;

public partial class MainForm : Form
{
    private readonly AppConfig _config;

    public MainForm(AppConfig config)
    {
        _config = config;
        InitializeComponent();

        // Phase 1: nothing else to do. Phase 2 will wire the Setup tab against _config.
        if (!_config.IsBootstrapped)
            _tabs.SelectedIndex = 0; // 초기 설정
    }
}
