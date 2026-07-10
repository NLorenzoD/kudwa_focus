using System.Drawing.Drawing2D;

namespace kudwa_focus;

internal sealed class stage_panel : Panel
{
    private readonly Color deep_colour = Color.FromArgb(14, 17, 16);
    private readonly Color orange_colour = Color.FromArgb(255, 91, 31);
    private readonly Color amber_colour = Color.FromArgb(255, 184, 0);
    private readonly Color red_colour = Color.FromArgb(255, 45, 45);
    private double progress_value;
    private display_phase current_phase = display_phase.ready;

    public stage_panel()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = deep_colour;
    }

    public void update_visual(double progress, display_phase phase)
    {
        var next_progress = Math.Clamp(progress, 0.0, 1.0);

        if (Math.Abs(progress_value - next_progress) < 0.0001 && current_phase == phase)
        {
            return;
        }

        progress_value = next_progress;
        current_phase = phase;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs event_args)
    {
        base.OnPaint(event_args);
        var graphics = event_args.Graphics;
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.Clear(deep_colour);

        var accent = current_phase switch
        {
            display_phase.final_ten or display_phase.complete => red_colour,
            display_phase.final_minute => amber_colour,
            _ => orange_colour
        };

        draw_angular_blocks(graphics, accent);
        draw_structure_lines(graphics, accent);
        draw_progress(graphics, accent);
    }

    private void draw_angular_blocks(Graphics graphics, Color accent)
    {
        using var faint_brush = new SolidBrush(Color.FromArgb(24, accent));
        using var bright_brush = new SolidBrush(Color.FromArgb(205, accent));
        using var dark_brush = new SolidBrush(Color.FromArgb(165, 5, 7, 7));

        var right_block = new[]
        {
            new Point(Math.Max(0, Width - 290), 0),
            new Point(Width, 0),
            new Point(Width, 245),
            new Point(Math.Max(0, Width - 160), 140)
        };
        graphics.FillPolygon(faint_brush, right_block);

        var lower_block = new[]
        {
            new Point(0, Math.Max(0, Height - 145)),
            new Point(260, Height),
            new Point(0, Height)
        };
        graphics.FillPolygon(faint_brush, lower_block);

        var marker_size = 34;
        var marker_left = Math.Max(18, Width - 92);
        graphics.TranslateTransform(marker_left, 42);
        graphics.RotateTransform(45F);
        graphics.FillRectangle(bright_brush, 0, 0, marker_size, marker_size);
        graphics.FillRectangle(dark_brush, 8, 8, marker_size - 16, marker_size - 16);
        graphics.ResetTransform();
    }

    private void draw_structure_lines(Graphics graphics, Color accent)
    {
        using var line_pen = new Pen(Color.FromArgb(22, accent), 2F);

        for (var x = -Height; x < Width; x += 64)
        {
            graphics.DrawLine(line_pen, x, Height, x + Height, 0);
        }
    }

    private void draw_progress(Graphics graphics, Color accent)
    {
        var track_height = Math.Max(10, (int)(Height * 0.018));
        var track_rectangle = new Rectangle(0, Height - track_height, Width, track_height);
        using var track_brush = new SolidBrush(Color.FromArgb(60, 255, 255, 255));
        using var progress_brush = new SolidBrush(accent);
        graphics.FillRectangle(track_brush, track_rectangle);
        graphics.FillRectangle(
            progress_brush,
            new Rectangle(0, Height - track_height, (int)(Width * progress_value), track_height));
    }
}
