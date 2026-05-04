using System.Drawing.Drawing2D;
using PZServerManager.Models;

namespace PZServerManager.Services;

public static class StatusIcons
{
    public static Color ColorFor(ServerStatus s) => s switch
    {
        ServerStatus.Running => Color.MediumSeaGreen,
        ServerStatus.Starting => Color.Goldenrod,
        ServerStatus.Stopping => Color.Goldenrod,
        ServerStatus.Crashed => Color.Crimson,
        _ => Color.DimGray,
    };

    public static Icon Create(ServerStatus s) => CreateColoredCircle(ColorFor(s));

    public static Icon CreateColoredCircle(Color color)
    {
        using var bmp = new Bitmap(32, 32);
        using (var g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using var brush = new SolidBrush(color);
            g.FillEllipse(brush, 3, 3, 26, 26);
            using var pen = new Pen(Color.FromArgb(60, 0, 0, 0), 1.5f);
            g.DrawEllipse(pen, 3, 3, 26, 26);
        }
        return Icon.FromHandle(bmp.GetHicon());
    }
}
