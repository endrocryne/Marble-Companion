using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace MarbleCompanion.Mobile.Controls;

public class ProgressRingControl : SKCanvasView
{
    public static readonly BindableProperty ProgressProperty =
        BindableProperty.Create(nameof(Progress), typeof(double), typeof(ProgressRingControl), 0.0,
            propertyChanged: OnPropertyChanged);

    public static readonly BindableProperty DisplayTextProperty =
        BindableProperty.Create(nameof(DisplayText), typeof(string), typeof(ProgressRingControl), string.Empty,
            propertyChanged: OnPropertyChanged);

    public static readonly BindableProperty RingColorProperty =
        BindableProperty.Create(nameof(RingColor), typeof(Color), typeof(ProgressRingControl),
            Color.FromArgb("#32E0C4"), propertyChanged: OnPropertyChanged);

    public static readonly BindableProperty TrackColorProperty =
        BindableProperty.Create(nameof(TrackColor), typeof(Color), typeof(ProgressRingControl),
            Color.FromArgb("#E0E0E0"), propertyChanged: OnPropertyChanged);

    public static readonly BindableProperty StrokeWidthProperty =
        BindableProperty.Create(nameof(StrokeWidth), typeof(float), typeof(ProgressRingControl), 12f,
            propertyChanged: OnPropertyChanged);

    public double Progress
    {
        get => (double)GetValue(ProgressProperty);
        set => SetValue(ProgressProperty, value);
    }

    public string DisplayText
    {
        get => (string)GetValue(DisplayTextProperty);
        set => SetValue(DisplayTextProperty, value);
    }

    public Color RingColor
    {
        get => (Color)GetValue(RingColorProperty);
        set => SetValue(RingColorProperty, value);
    }

    public Color TrackColor
    {
        get => (Color)GetValue(TrackColorProperty);
        set => SetValue(TrackColorProperty, value);
    }

    public float StrokeWidth
    {
        get => (float)GetValue(StrokeWidthProperty);
        set => SetValue(StrokeWidthProperty, value);
    }

    public ProgressRingControl()
    {
        PaintSurface += OnPaintSurface;
    }

    private static void OnPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ProgressRingControl control)
            control.InvalidateSurface();
    }

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        canvas.Clear();

        float size = Math.Min(info.Width, info.Height);
        float cx = info.Width / 2f;
        float cy = info.Height / 2f;
        float radius = (size - StrokeWidth) / 2f;

        var rect = new SKRect(cx - radius, cy - radius, cx + radius, cy + radius);

        // Track
        using var trackPaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = StrokeWidth,
            Color = ToSkColor(TrackColor),
            IsAntialias = true,
            StrokeCap = SKStrokeCap.Round
        };
        canvas.DrawCircle(cx, cy, radius, trackPaint);

        // Progress arc
        float sweepAngle = (float)(Math.Clamp(Progress, 0, 1) * 360);
        if (sweepAngle > 0)
        {
            using var ringPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeWidth = StrokeWidth,
                Color = ToSkColor(RingColor),
                IsAntialias = true,
                StrokeCap = SKStrokeCap.Round
            };

            using var path = new SKPath();
            path.AddArc(rect, -90, sweepAngle);
            canvas.DrawPath(path, ringPaint);
        }

        // Center text
        if (!string.IsNullOrEmpty(DisplayText))
        {
            using var textPaint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center,
                TextSize = radius * 0.35f
            };
            var textBounds = new SKRect();
            textPaint.MeasureText(DisplayText, ref textBounds);
            canvas.DrawText(DisplayText, cx, cy - textBounds.MidY, textPaint);
        }
    }

    private static SKColor ToSkColor(Color color) =>
        new((byte)(color.Red * 255), (byte)(color.Green * 255),
            (byte)(color.Blue * 255), (byte)(color.Alpha * 255));
}
