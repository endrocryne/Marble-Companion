using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace MarbleCompanion.Mobile.Controls;

public record DonutSegment(string Label, decimal Value, Color Color);

public class DonutChartControl : SKCanvasView
{
    public static readonly BindableProperty SegmentsProperty =
        BindableProperty.Create(nameof(Segments), typeof(List<DonutSegment>), typeof(DonutChartControl), null,
            propertyChanged: OnPropertyChanged);

    public static readonly BindableProperty InnerRadiusRatioProperty =
        BindableProperty.Create(nameof(InnerRadiusRatio), typeof(double), typeof(DonutChartControl), 0.6,
            propertyChanged: OnPropertyChanged);

    public List<DonutSegment>? Segments
    {
        get => (List<DonutSegment>?)GetValue(SegmentsProperty);
        set => SetValue(SegmentsProperty, value);
    }

    public double InnerRadiusRatio
    {
        get => (double)GetValue(InnerRadiusRatioProperty);
        set => SetValue(InnerRadiusRatioProperty, value);
    }

    public DonutChartControl()
    {
        PaintSurface += OnPaintSurface;
    }

    private static void OnPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is DonutChartControl control)
            control.InvalidateSurface();
    }

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        canvas.Clear();

        var segments = Segments;
        if (segments is null || segments.Count == 0) return;

        float size = Math.Min(info.Width, info.Height);
        float cx = info.Width / 2f;
        float cy = info.Height / 2f;
        float outerRadius = size / 2f - 10f;
        float innerRadius = outerRadius * (float)Math.Clamp(InnerRadiusRatio, 0.1, 0.95);

        decimal total = segments.Sum(s => s.Value);
        if (total <= 0) return;

        const float gapDegrees = 2f;
        float totalGap = gapDegrees * segments.Count;
        float availableDegrees = 360f - totalGap;
        float startAngle = -90f;

        var outerRect = new SKRect(cx - outerRadius, cy - outerRadius, cx + outerRadius, cy + outerRadius);
        var innerRect = new SKRect(cx - innerRadius, cy - innerRadius, cx + innerRadius, cy + innerRadius);

        foreach (var segment in segments)
        {
            float sweepAngle = (float)((double)(segment.Value / total)) * availableDegrees;

            if (sweepAngle < 0.1f)
            {
                startAngle += sweepAngle + gapDegrees;
                continue;
            }

            using var path = new SKPath();
            path.ArcTo(outerRect, startAngle, sweepAngle, true);
            path.ArcTo(innerRect, startAngle + sweepAngle, -sweepAngle, false);
            path.Close();

            using var paint = new SKPaint
            {
                Color = ToSkColor(segment.Color),
                IsAntialias = true,
                Style = SKPaintStyle.Fill
            };
            canvas.DrawPath(path, paint);

            startAngle += sweepAngle + gapDegrees;
        }

        // Center hole (clean fill over any antialiasing artifacts)
        using var holePaint = new SKPaint
        {
            Color = SKColors.White,
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };
        canvas.DrawCircle(cx, cy, innerRadius - 0.5f, holePaint);

        // Total text in center
        using var textPaint = new SKPaint
        {
            Color = new SKColor(50, 50, 50),
            IsAntialias = true,
            TextAlign = SKTextAlign.Center,
            TextSize = innerRadius * 0.35f,
            FakeBoldText = true
        };
        string totalText = total.ToString("F1");
        var textBounds = new SKRect();
        textPaint.MeasureText(totalText, ref textBounds);
        canvas.DrawText(totalText, cx, cy - textBounds.MidY, textPaint);
    }

    private static SKColor ToSkColor(Color color) =>
        new((byte)(color.Red * 255), (byte)(color.Green * 255),
            (byte)(color.Blue * 255), (byte)(color.Alpha * 255));
}
