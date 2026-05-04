using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace PZServerManager.Forms.Controls;

public sealed class StepCard : Panel
{
    private bool _active;
    private bool _done;

    public StepCard()
    {
        BackColor = Color.White;
        SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                 | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        Padding = new Padding(20, 18, 20, 18);
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsActive
    {
        get => _active;
        set { if (_active == value) return; _active = value; Invalidate(); }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsDone
    {
        get => _done;
        set { if (_done == value) return; _done = value; Invalidate(); }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        var radius = 8;
        using var path = new GraphicsPath();
        path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
        path.AddArc(rect.Right - radius, rect.Y, radius, radius, 270, 90);
        path.AddArc(rect.Right - radius, rect.Bottom - radius, radius, radius, 0, 90);
        path.AddArc(rect.X, rect.Bottom - radius, radius, radius, 90, 90);
        path.CloseFigure();

        Color border = _active ? Color.FromArgb(74, 122, 250)
                       : _done ? Color.FromArgb(220, 234, 224)
                       : Color.FromArgb(225, 227, 232);
        using var pen = new Pen(border, _active ? 1.6f : 1f);
        g.DrawPath(pen, path);
    }
}
