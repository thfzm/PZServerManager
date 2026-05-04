namespace PZServerManager.Forms;

public static class PromptDialog
{
    public static string? Ask(IWin32Window owner, string title, string label, string defaultValue,
        bool isPassword = false)
    {
        using var f = new Form
        {
            Text = title,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            StartPosition = FormStartPosition.CenterParent,
            ClientSize = new Size(420, 140),
        };

        var lbl = new Label { Text = label, AutoSize = true, Location = new Point(12, 12) };
        var tb = new TextBox
        {
            Location = new Point(12, 38),
            Size = new Size(396, 23),
            Text = defaultValue,
            UseSystemPasswordChar = isPassword,
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
        };
        var ok = new Button
        {
            Text = "OK",
            DialogResult = DialogResult.OK,
            Location = new Point(244, 92),
            Size = new Size(80, 28),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
        };
        var cancel = new Button
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel,
            Location = new Point(330, 92),
            Size = new Size(80, 28),
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
        };
        f.Controls.Add(lbl);
        f.Controls.Add(tb);
        f.Controls.Add(ok);
        f.Controls.Add(cancel);
        f.AcceptButton = ok;
        f.CancelButton = cancel;

        return f.ShowDialog(owner) == DialogResult.OK ? tb.Text : null;
    }
}
