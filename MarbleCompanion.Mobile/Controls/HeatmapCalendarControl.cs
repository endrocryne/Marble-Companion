using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace MarbleCompanion.Mobile.Controls;

public class HeatmapCalendarControl : SKCanvasView
{
    private const int DaysInWeek = 7;
    private const float LabelWidth = 24f;
    private const float CellPadding = 3f;
    private const float CornerRadius = 2f;

    public static readonly BindableProperty DataProperty =
        BindableProperty.Create(nameof(Data), typeof(Dictionary<DateTime, int>), typeof(HeatmapCalendarControl), null,
            propertyChanged: OnPropertyChanged);

    public static readonly BindableProperty WeeksToShowProperty =
        BindableProperty.Create(nameof(WeeksToShow), typeof(int), typeof(HeatmapCalendarControl), 12,
            propertyChanged: OnPropertyChanged);

    public static readonly BindableProperty BaseColorProperty =
        BindableProperty.Create(nameof(BaseColor), typeof(Color), typeof(HeatmapCalendarControl),
            Color.FromArgb("#3AB795"), propertyChanged: OnPropertyChanged);

    public Dictionary<DateTime, int>? Data
    {
        get => (Dictionary<DateTime, int>?)GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    public int WeeksToShow
    {
        get => (int)GetValue(WeeksToShowProperty);
        set => SetValue(WeeksToShowProperty, value);
    }

    public Color BaseColor
    {
        get => (Color)GetValue(BaseColorProperty);
        set => SetValue(BaseColorProperty, value);
    }

    public HeatmapCalendarControl()
    {
        PaintSurface += OnPaintSurface;
    }

    private static void OnPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is HeatmapCalendarControl control)
            control.InvalidateSurface();
    }

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        canvas.Clear();

        int weeks = Math.Max(1, WeeksToShow);
        float availableWidth = info.Width - LabelWidth;
        float availableHeight = info.Height;
        float cellSize = Math.Min(
            (availableWidth - CellPadding * weeks) / weeks,
            (availableHeight - CellPadding * DaysInWeek) / DaysInWeek);
        cellSize = Math.Max(cellSize, 4f);

        var data = Data ?? new Dictionary<DateTime, int>();
        int maxCount = data.Count > 0 ? data.Values.Max() : 1;
        if (maxCount == 0) maxCount = 1;

        var baseSkColor = ToSkColor(BaseColor);
        var emptyColor = new SKColor(235, 235, 235);

        // Day labels on left side (M, W, F)
        DrawDayLabels(canvas, cellSize);

        // Compute the start date: end of the most recent week going back WeeksToShow weeks
        var today = DateTime.Today;
        int daysSinceSunday = (int)today.DayOfWeek;
        var endOfWeek = today.AddDays(6 - daysSinceSunday);
        var startDate = endOfWeek.AddDays(-((weeks * 7) - 1));

        // Draw cells
        using var paint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };

        for (int week = 0; week < weeks; week++)
        {
            for (int day = 0; day < DaysInWeek; day++)
            {
                var cellDate = startDate.AddDays(week * 7 + day);
                float x = LabelWidth + week * (cellSize + CellPadding);
                float y = day * (cellSize + CellPadding);

                if (cellDate > today)
                {
                    // Future dates: skip
                    continue;
                }

                data.TryGetValue(cellDate.Date, out int count);

                if (count > 0)
                {
                    float intensity = Math.Clamp((float)count / maxCount, 0.2f, 1f);
                    paint.Color = new SKColor(
                        (byte)(baseSkColor.Red * intensity + 235 * (1 - intensity)),
                        (byte)(baseSkColor.Green * intensity + 235 * (1 - intensity)),
                        (byte)(baseSkColor.Blue * intensity + 235 * (1 - intensity)),
                        255);
                }
                else
                {
                    paint.Color = emptyColor;
                }

                var rect = new SKRoundRect(new SKRect(x, y, x + cellSize, y + cellSize), CornerRadius);
                canvas.DrawRoundRect(rect, paint);
            }
        }
    }

    private static void DrawDayLabels(SKCanvas canvas, float cellSize)
    {
        using var paint = new SKPaint
        {
            Color = new SKColor(130, 130, 130),
            IsAntialias = true,
            TextSize = Math.Min(cellSize * 0.7f, 11f),
            TextAlign = SKTextAlign.Left
        };

        // Show labels for Mon (index 1), Wed (index 3), Fri (index 5)
        (string label, int row)[] labels = { ("M", 1), ("W", 3), ("F", 5) };

        foreach (var (label, row) in labels)
        {
            float y = row * (cellSize + CellPadding) + cellSize * 0.75f;
            canvas.DrawText(label, 4, y, paint);
        }
    }

    private static SKColor ToSkColor(Color color) =>
        new((byte)(color.Red * 255), (byte)(color.Green * 255),
            (byte)(color.Blue * 255), (byte)(color.Alpha * 255));
}
