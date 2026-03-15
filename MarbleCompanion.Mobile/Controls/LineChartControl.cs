using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace MarbleCompanion.Mobile.Controls;

public record ChartDataPoint(DateTime Date, decimal Value);

public class LineChartControl : SKCanvasView
{
    private const float Padding = 40f;

    public static readonly BindableProperty DataPointsProperty =
        BindableProperty.Create(nameof(DataPoints), typeof(List<ChartDataPoint>), typeof(LineChartControl), null,
            propertyChanged: OnPropertyChanged);

    public static readonly BindableProperty LineColorProperty =
        BindableProperty.Create(nameof(LineColor), typeof(Color), typeof(LineChartControl),
            Color.FromArgb("#0D7377"), propertyChanged: OnPropertyChanged);

    public static readonly BindableProperty FillColorProperty =
        BindableProperty.Create(nameof(FillColor), typeof(Color), typeof(LineChartControl), null,
            propertyChanged: OnPropertyChanged);

    public static readonly BindableProperty ShowGridProperty =
        BindableProperty.Create(nameof(ShowGrid), typeof(bool), typeof(LineChartControl), true,
            propertyChanged: OnPropertyChanged);

    public List<ChartDataPoint>? DataPoints
    {
        get => (List<ChartDataPoint>?)GetValue(DataPointsProperty);
        set => SetValue(DataPointsProperty, value);
    }

    public Color LineColor
    {
        get => (Color)GetValue(LineColorProperty);
        set => SetValue(LineColorProperty, value);
    }

    public Color? FillColor
    {
        get => (Color?)GetValue(FillColorProperty);
        set => SetValue(FillColorProperty, value);
    }

    public bool ShowGrid
    {
        get => (bool)GetValue(ShowGridProperty);
        set => SetValue(ShowGridProperty, value);
    }

    public LineChartControl()
    {
        PaintSurface += OnPaintSurface;
    }

    private static void OnPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is LineChartControl control)
            control.InvalidateSurface();
    }

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        canvas.Clear();

        var points = DataPoints;
        if (points is null || points.Count < 2) return;

        float w = info.Width;
        float h = info.Height;
        float chartLeft = Padding * 1.5f;
        float chartRight = w - Padding;
        float chartTop = Padding;
        float chartBottom = h - Padding * 1.5f;
        float chartWidth = chartRight - chartLeft;
        float chartHeight = chartBottom - chartTop;

        var sorted = points.OrderBy(p => p.Date).ToList();
        decimal minVal = sorted.Min(p => p.Value);
        decimal maxVal = sorted.Max(p => p.Value);
        if (minVal == maxVal) { minVal -= 1; maxVal += 1; }
        decimal valRange = maxVal - minVal;

        long minTicks = sorted[0].Date.Ticks;
        long maxTicks = sorted[^1].Date.Ticks;
        long tickRange = maxTicks - minTicks;
        if (tickRange == 0) tickRange = 1;

        SKPoint ToCanvas(ChartDataPoint p)
        {
            float x = chartLeft + (float)((p.Date.Ticks - minTicks) / (double)tickRange) * chartWidth;
            float y = chartBottom - (float)((double)(p.Value - minVal) / (double)valRange) * chartHeight;
            return new SKPoint(x, y);
        }

        // Grid
        if (ShowGrid)
            DrawGrid(canvas, chartLeft, chartTop, chartRight, chartBottom, minVal, maxVal);

        // Axes
        using (var axisPaint = new SKPaint
        {
            Color = new SKColor(100, 100, 100),
            StrokeWidth = 1.5f,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke
        })
        {
            canvas.DrawLine(chartLeft, chartTop, chartLeft, chartBottom, axisPaint);
            canvas.DrawLine(chartLeft, chartBottom, chartRight, chartBottom, axisPaint);
        }

        // Build smooth bezier path through data points
        var canvasPoints = sorted.Select(ToCanvas).ToArray();

        using var linePath = new SKPath();
        linePath.MoveTo(canvasPoints[0]);

        for (int i = 0; i < canvasPoints.Length - 1; i++)
        {
            var p0 = i > 0 ? canvasPoints[i - 1] : canvasPoints[i];
            var p1 = canvasPoints[i];
            var p2 = canvasPoints[i + 1];
            var p3 = i + 2 < canvasPoints.Length ? canvasPoints[i + 2] : canvasPoints[i + 1];

            float tension = 0.3f;
            float cp1x = p1.X + (p2.X - p0.X) * tension;
            float cp1y = p1.Y + (p2.Y - p0.Y) * tension;
            float cp2x = p2.X - (p3.X - p1.X) * tension;
            float cp2y = p2.Y - (p3.Y - p1.Y) * tension;

            linePath.CubicTo(cp1x, cp1y, cp2x, cp2y, p2.X, p2.Y);
        }

        // Gradient fill below line
        var fillColor = FillColor ?? LineColor;
        var skFill = ToSkColor(fillColor);
        using (var fillPath = new SKPath(linePath))
        {
            fillPath.LineTo(canvasPoints[^1].X, chartBottom);
            fillPath.LineTo(canvasPoints[0].X, chartBottom);
            fillPath.Close();

            using var shader = SKShader.CreateLinearGradient(
                new SKPoint(0, chartTop), new SKPoint(0, chartBottom),
                new[] { skFill.WithAlpha(120), skFill.WithAlpha(10) },
                null, SKShaderTileMode.Clamp);
            using var fillPaint = new SKPaint { Shader = shader, IsAntialias = true, Style = SKPaintStyle.Fill };
            canvas.DrawPath(fillPath, fillPaint);
        }

        // Line
        using var linePaint = new SKPaint
        {
            Color = ToSkColor(LineColor),
            StrokeWidth = 2.5f,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeCap = SKStrokeCap.Round
        };
        canvas.DrawPath(linePath, linePaint);
    }

    private static void DrawGrid(SKCanvas canvas, float left, float top, float right, float bottom,
        decimal minVal, decimal maxVal)
    {
        using var paint = new SKPaint
        {
            Color = new SKColor(220, 220, 220),
            StrokeWidth = 0.5f,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke
        };
        using var labelPaint = new SKPaint
        {
            Color = new SKColor(130, 130, 130),
            TextSize = 10,
            IsAntialias = true
        };

        const int gridLines = 5;
        float chartHeight = bottom - top;

        for (int i = 0; i <= gridLines; i++)
        {
            float y = top + chartHeight * i / gridLines;
            canvas.DrawLine(left, y, right, y, paint);

            decimal val = maxVal - (maxVal - minVal) * i / gridLines;
            canvas.DrawText(val.ToString("F1"), left - 35, y + 4, labelPaint);
        }
    }

    private static SKColor ToSkColor(Color color) =>
        new((byte)(color.Red * 255), (byte)(color.Green * 255),
            (byte)(color.Blue * 255), (byte)(color.Alpha * 255));
}
