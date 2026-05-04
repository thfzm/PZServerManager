namespace PZServerManager.Forms;

public static class PickBackupDialog
{
    public static string? Pick(IWin32Window owner, IReadOnlyList<string> backups)
    {
        if (backups.Count == 0) return null;

        using var f = new Form
        {
            Text = "Pick a backup",
            FormBorderStyle = FormBorderStyle.Sizable,
            StartPosition = FormStartPosition.CenterParent,
            ClientSize = new Size(560, 380),
            MinimumSize = new Size(420, 280),
        };
        var list = new ListBox
        {
            Dock = DockStyle.Fill,
            IntegralHeight = false,
            Font = new Font("Consolas", 9.5f),
        };
        foreach (var b in backups)
        {
            var fi = new FileInfo(b);
            list.Items.Add($"{fi.LastWriteTime:yyyy-MM-dd HH:mm}   {fi.Length / 1024 / 1024,6} MB   {fi.Name}");
        }
        list.SelectedIndex = 0;

        var bottom = new Panel { Dock = DockStyle.Bottom, Height = 44, Padding = new Padding(8) };
        var ok = new Button
        {
            Text = "Restore",
            DialogResult = DialogResult.OK,
            Anchor = AnchorStyles.Right,
            Size = new Size(100, 28),
            Location = new Point(bottom.ClientSize.Width - 220, 8),
        };
        var cancel = new Button
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel,
            Anchor = AnchorStyles.Right,
            Size = new Size(100, 28),
            Location = new Point(bottom.ClientSize.Width - 110, 8),
        };
        bottom.Controls.Add(ok);
        bottom.Controls.Add(cancel);

        f.Controls.Add(list);
        f.Controls.Add(bottom);
        f.AcceptButton = ok;
        f.CancelButton = cancel;

        return f.ShowDialog(owner) == DialogResult.OK ? backups[list.SelectedIndex] : null;
    }
}
