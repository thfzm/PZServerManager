using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace PZServerManager.Forms.Controls;

public sealed class StepBadge : Control
{
    public enum BadgeState { Inactive, Active, Done }

    private int _number = 1;
    private BadgeState _state = BadgeState.Inactive;

    public StepBadge()
    {
        Size = new Size(34, 34);
        DoubleBuffered = true;
        SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.UserPaint
                 | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        BackColor = Color.Transparent;
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int Number
    {
        get => _number;
        set { if (_number == value) return; _number = value; Invalidate(); }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public BadgeState State
    {
        get => _state;
        set { if (_state == value) return; _state = value; Invalidate(); }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        var rect = new Rectangle(1, 1, Width - 3, Height - 3);
        Color fill = _state switch
        {
            BadgeState.Active => Color.FromArgb(74, 122, 250),
            BadgeState.Done => Color.FromArgb(46, 160, 67),
            _ => Color.FromArgb(220, 222, 226),
        };
        using (var brush = new SolidBrush(fill))
            g.FillEllipse(brush, rect);

        if (_state == BadgeState.Done)
        {
            using var pen = new Pen(Color.White, 2.6f) { StartCap = LineCap.Round, EndCap = LineCap.Round };
            var cx = rect.Left + rect.Width / 2f;
            var cy = rect.Top + rect.Height / 2f;
            g.DrawLines(pen, new[]
            {
                new PointF(cx - 6, cy + 0.5f),
                new PointF(cx - 1.5f, cy + 5),
                new PointF(cx + 7, cy - 4),
            });
        }
        else
        {
            using var f = new Font("Segoe UI", 11f, FontStyle.Bold);
            var text = _number.ToString();
            var size = g.MeasureString(text, f);
            using var brush = new SolidBrush(_state == BadgeState.Active
                ? Color.White
                : Color.FromArgb(120, 120, 120));
            g.DrawString(text, f, brush,
                rect.Left + (rect.Width - size.Width) / 2f,
                rect.Top + (rect.Height - size.Height) / 2f - 1);
        }
    }
}
