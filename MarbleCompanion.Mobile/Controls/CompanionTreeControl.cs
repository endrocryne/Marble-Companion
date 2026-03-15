using MarbleCompanion.Mobile.Models;
using MarbleCompanion.Shared.Enums;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;

namespace MarbleCompanion.Mobile.Controls;

public class CompanionTreeControl : SKCanvasView
{
    private static readonly Random _random = new(42);
    private bool _breatheIn = true;
    private float _breatheScale = 1.0f;

    public static readonly BindableProperty RenderStateProperty =
        BindableProperty.Create(
            nameof(RenderState),
            typeof(TreeRenderState),
            typeof(CompanionTreeControl),
            null,
            propertyChanged: OnRenderStateChanged);

    public TreeRenderState? RenderState
    {
        get => (TreeRenderState?)GetValue(RenderStateProperty);
        set => SetValue(RenderStateProperty, value);
    }

    public CompanionTreeControl()
    {
        PaintSurface += OnPaintSurface;
        StartBreathingAnimation();
    }

    private static void OnRenderStateChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is CompanionTreeControl control)
            control.InvalidateSurface();
    }

    private void StartBreathingAnimation()
    {
        Dispatcher.StartTimer(TimeSpan.FromMilliseconds(50), () =>
        {
            const float step = 0.002f;
            if (_breatheIn)
            {
                _breatheScale += step;
                if (_breatheScale >= 1.02f) _breatheIn = false;
            }
            else
            {
                _breatheScale -= step;
                if (_breatheScale <= 0.98f) _breatheIn = true;
            }

            InvalidateSurface();
            return true;
        });
    }

    private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        var info = e.Info;
        canvas.Clear();

        var state = RenderState;
        if (state is null) return;

        var w = info.Width;
        var h = info.Height;

        canvas.Save();
        canvas.Translate(w / 2f, h / 2f);
        canvas.Scale(_breatheScale);
        canvas.Translate(-w / 2f, -h / 2f);

        DrawSky(canvas, w, h, state);
        DrawGround(canvas, w, h, state);
        DrawSeasonElements(canvas, w, h, state);
        DrawTree(canvas, w, h, state);

        if (state.Stage >= 7)
            DrawStageDecorations(canvas, w, h);

        if (state.HasCompanionCreature)
            DrawCompanion(canvas, w, h, state.CompanionCreatureType);

        canvas.Restore();
    }

    #region Sky

    private static void DrawSky(SKCanvas canvas, int w, int h, TreeRenderState state)
    {
        var skyTheme = ResolveSkyTheme(state);
        SKColor topColor, bottomColor;

        switch (skyTheme)
        {
            case "Dawn":
                topColor = new SKColor(255, 154, 80);
                bottomColor = new SKColor(255, 182, 193);
                break;
            case "Dusk":
                topColor = new SKColor(75, 0, 130);
                bottomColor = new SKColor(25, 25, 112);
                break;
            case "Starfield":
                topColor = new SKColor(10, 10, 35);
                bottomColor = new SKColor(20, 20, 60);
                break;
            case "NorthernLights":
                topColor = new SKColor(0, 200, 100);
                bottomColor = new SKColor(128, 0, 255);
                break;
            default: // Day
                topColor = new SKColor(135, 206, 235);
                bottomColor = new SKColor(200, 230, 255);
                break;
        }

        using var shader = SKShader.CreateLinearGradient(
            new SKPoint(0, 0), new SKPoint(0, h * 0.8f),
            new[] { topColor, bottomColor },
            null, SKShaderTileMode.Clamp);
        using var paint = new SKPaint { Shader = shader };
        canvas.DrawRect(0, 0, w, h, paint);

        if (skyTheme == "Starfield")
            DrawStars(canvas, w, h);
    }

    private static void DrawStars(SKCanvas canvas, int w, int h)
    {
        using var paint = new SKPaint { Color = SKColors.White, IsAntialias = true };
        var rng = new Random(123);
        for (int i = 0; i < 60; i++)
        {
            float x = rng.Next(w);
            float y = rng.Next((int)(h * 0.7f));
            float r = 0.5f + (float)rng.NextDouble() * 1.5f;
            canvas.DrawCircle(x, y, r, paint);
        }
    }

    private static string ResolveSkyTheme(TreeRenderState state)
    {
        foreach (var c in state.ActiveCosmetics)
        {
            if (c is "Dawn" or "Dusk" or "Starfield" or "NorthernLights")
                return c;
        }
        return "Default";
    }

    #endregion

    #region Ground

    private static void DrawGround(SKCanvas canvas, int w, int h, TreeRenderState state)
    {
        var groundTheme = ResolveGroundTheme(state);
        var color = groundTheme switch
        {
            "Rocky" => new SKColor(139, 119, 101),
            "Coastal" => new SKColor(238, 214, 175),
            "Desert" => new SKColor(210, 180, 140),
            "Tundra" => new SKColor(240, 248, 255),
            _ => new SKColor(76, 153, 0) // Meadow
        };

        float groundTop = h * 0.8f;
        using var paint = new SKPaint { Color = color, IsAntialias = true };
        canvas.DrawRect(0, groundTop, w, h - groundTop, paint);
    }

    private static string ResolveGroundTheme(TreeRenderState state)
    {
        foreach (var c in state.ActiveCosmetics)
        {
            if (c is "Meadow" or "Rocky" or "Coastal" or "Desert" or "Tundra")
                return c;
        }
        return "Meadow";
    }

    #endregion

    #region Season

    private static void DrawSeasonElements(SKCanvas canvas, int w, int h, TreeRenderState state)
    {
        var rng = new Random(77);
        switch (state.CurrentSeason)
        {
            case Season.Spring:
                using (var paint = new SKPaint { Color = new SKColor(255, 182, 193, 180), IsAntialias = true })
                {
                    for (int i = 0; i < 20; i++)
                        canvas.DrawCircle(rng.Next(w), rng.Next((int)(h * 0.7f)), 3 + rng.Next(4), paint);
                }
                break;

            case Season.Summer:
                using (var paint = new SKPaint { Color = new SKColor(255, 255, 100, 30) })
                    canvas.DrawRect(0, 0, w, h, paint);
                break;

            case Season.Autumn:
                using (var paint = new SKPaint { IsAntialias = true })
                {
                    SKColor[] autumnColors = { new(210, 105, 30), new(255, 140, 0), new(139, 69, 19) };
                    for (int i = 0; i < 25; i++)
                    {
                        paint.Color = autumnColors[rng.Next(autumnColors.Length)];
                        float x = rng.Next(w);
                        float y = rng.Next((int)(h * 0.8f));
                        canvas.DrawRect(x, y, 4, 4, paint);
                    }
                }
                break;

            case Season.Winter:
                using (var paint = new SKPaint { Color = new SKColor(255, 255, 255, 200), IsAntialias = true })
                {
                    for (int i = 0; i < 30; i++)
                        canvas.DrawCircle(rng.Next(w), rng.Next(h), 2 + rng.Next(3), paint);
                }
                break;
        }
    }

    #endregion

    #region Tree

    private static void DrawTree(SKCanvas canvas, int w, int h, TreeRenderState state)
    {
        int stage = state.Stage;
        float trunkWidth = 4 + stage * 3;
        float trunkHeight = h * 0.1f + stage * h * 0.05f;
        float trunkBaseY = h * 0.8f;
        float trunkTopY = trunkBaseY - trunkHeight;
        float cx = w / 2f;

        using var trunkPaint = new SKPaint
        {
            Color = new SKColor(101, 67, 33),
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };

        using var path = new SKPath();
        path.MoveTo(cx - trunkWidth / 2f, trunkBaseY);
        path.CubicTo(
            cx - trunkWidth / 3f, trunkBaseY - trunkHeight * 0.3f,
            cx - trunkWidth / 4f, trunkBaseY - trunkHeight * 0.7f,
            cx, trunkTopY);
        path.CubicTo(
            cx + trunkWidth / 4f, trunkBaseY - trunkHeight * 0.7f,
            cx + trunkWidth / 3f, trunkBaseY - trunkHeight * 0.3f,
            cx + trunkWidth / 2f, trunkBaseY);
        path.Close();
        canvas.DrawPath(path, trunkPaint);

        // Branches
        int maxDepth = Math.Min(stage, 6);
        if (maxDepth > 0)
        {
            using var branchPaint = new SKPaint
            {
                Color = new SKColor(101, 67, 33),
                IsAntialias = true,
                StrokeCap = SKStrokeCap.Round,
                Style = SKPaintStyle.Stroke
            };

            float branchLength = trunkHeight * 0.45f;
            DrawBranch(canvas, cx, trunkTopY, branchLength, -90, maxDepth, branchPaint, state);
        }
    }

    private static void DrawBranch(SKCanvas canvas, float x, float y, float length, float angle,
        int depth, SKPaint paint, TreeRenderState state)
    {
        if (depth <= 0) return;

        paint.StrokeWidth = depth * 1.5f;

        float rad = angle * MathF.PI / 180f;
        float endX = x + MathF.Cos(rad) * length;
        float endY = y + MathF.Sin(rad) * length;

        canvas.DrawLine(x, y, endX, endY, paint);

        float spread = GetSpeciesSpread(state.Species);
        float nextLength = length * 0.68f;

        if (state.Species == TreeSpecies.Willow)
        {
            // Willow branches droop downward
            DrawBranch(canvas, endX, endY, nextLength, angle - spread + 15, depth - 1, paint, state);
            DrawBranch(canvas, endX, endY, nextLength, angle + spread + 15, depth - 1, paint, state);
        }
        else
        {
            DrawBranch(canvas, endX, endY, nextLength, angle - spread, depth - 1, paint, state);
            DrawBranch(canvas, endX, endY, nextLength, angle + spread, depth - 1, paint, state);
        }

        // Mangrove aerial roots at lower branches
        if (state.Species == TreeSpecies.Mangrove && depth <= 2 && angle > -60)
        {
            using var rootPaint = new SKPaint
            {
                Color = new SKColor(101, 67, 33, 150),
                IsAntialias = true,
                StrokeWidth = 1.5f,
                Style = SKPaintStyle.Stroke
            };
            canvas.DrawLine(endX, endY, endX, endY + length * 0.6f, rootPaint);
        }

        // Leaves at endpoints
        if (depth == 1)
            DrawLeafCluster(canvas, endX, endY, state);
    }

    private static float GetSpeciesSpread(TreeSpecies species) => species switch
    {
        TreeSpecies.Oak => 35f,
        TreeSpecies.Willow => 55f,
        TreeSpecies.Baobab => 20f,
        TreeSpecies.Maple => 40f,
        TreeSpecies.CherryBlossom => 45f,
        TreeSpecies.Mangrove => 30f,
        _ => 35f
    };

    private static void DrawLeafCluster(SKCanvas canvas, float x, float y, TreeRenderState state)
    {
        if (state.HealthState == TreeHealthState.Dormant) return;

        var color = GetLeafColor(state);
        using var paint = new SKPaint { Color = color, IsAntialias = true, Style = SKPaintStyle.Fill };

        var rng = new Random((int)(x * 100 + y));
        int count = 5 + rng.Next(4);
        for (int i = 0; i < count; i++)
        {
            float ox = (rng.Next(16) - 8);
            float oy = (rng.Next(16) - 8);
            float rx = 3 + rng.Next(3);
            float ry = 2 + rng.Next(2);
            canvas.DrawOval(x + ox, y + oy, rx, ry, paint);
        }
    }

    private static SKColor GetLeafColor(TreeRenderState state)
    {
        // Species-specific overrides
        var speciesColor = state.Species switch
        {
            TreeSpecies.CherryBlossom => new SKColor(255, 182, 193),
            TreeSpecies.Maple => new SKColor(210, 90, 30),
            TreeSpecies.Willow => new SKColor(144, 238, 144),
            _ => (SKColor?)null
        };

        if (speciesColor.HasValue && state.HealthState == TreeHealthState.Healthy)
            return speciesColor.Value;

        return state.HealthState switch
        {
            TreeHealthState.Healthy => new SKColor(34, 139, 34),   // #228B22
            TreeHealthState.Stressed => new SKColor(154, 205, 50), // #9ACD32
            TreeHealthState.Withering => new SKColor(139, 69, 19), // #8B4513
            _ => new SKColor(34, 139, 34)
        };
    }

    #endregion

    #region Decorations (Stage 7+)

    private static void DrawStageDecorations(SKCanvas canvas, int w, int h)
    {
        var rng = new Random(999);

        // Fireflies - glowing yellow dots
        using (var paint = new SKPaint { Color = new SKColor(255, 255, 100, 200), IsAntialias = true })
        using (var glowPaint = new SKPaint { Color = new SKColor(255, 255, 100, 60), IsAntialias = true })
        {
            for (int i = 0; i < 8; i++)
            {
                float fx = rng.Next(w);
                float fy = rng.Next((int)(h * 0.6f)) + h * 0.1f;
                canvas.DrawCircle(fx, fy, 4, glowPaint);
                canvas.DrawCircle(fx, fy, 2, paint);
            }
        }

        // V-shaped birds
        using (var paint = new SKPaint
        {
            Color = new SKColor(60, 60, 60),
            IsAntialias = true,
            StrokeWidth = 1.5f,
            Style = SKPaintStyle.Stroke,
            StrokeCap = SKStrokeCap.Round
        })
        {
            for (int i = 0; i < 4; i++)
            {
                float bx = rng.Next(w);
                float by = rng.Next((int)(h * 0.3f));
                float size = 6 + rng.Next(6);
                canvas.DrawLine(bx - size, by + size * 0.4f, bx, by, paint);
                canvas.DrawLine(bx, by, bx + size, by + size * 0.4f, paint);
            }
        }

        // Tiny flowers at tree base
        float groundY = h * 0.8f;
        using (var paint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill })
        {
            SKColor[] flowerColors = { SKColors.Red, SKColors.Yellow, new SKColor(255, 105, 180), SKColors.Orange };
            for (int i = 0; i < 10; i++)
            {
                paint.Color = flowerColors[rng.Next(flowerColors.Length)];
                float fx = w * 0.2f + rng.Next((int)(w * 0.6f));
                float fy = groundY - 2 - rng.Next(8);
                canvas.DrawCircle(fx, fy, 3, paint);
            }
        }
    }

    #endregion

    #region Companion Creatures

    private static void DrawCompanion(SKCanvas canvas, int w, int h, string creatureType)
    {
        float groundY = h * 0.8f;
        float cx = w * 0.7f;
        float cy = groundY - 10;

        switch (creatureType)
        {
            case "Bee":
                DrawBee(canvas, cx, cy);
                break;
            case "Robin":
                DrawRobin(canvas, cx, cy);
                break;
            case "Hedgehog":
                DrawHedgehog(canvas, cx, cy);
                break;
            case "Frog":
                DrawFrog(canvas, cx, cy);
                break;
            case "Butterfly":
                DrawButterfly(canvas, cx, cy);
                break;
        }
    }

    private static void DrawBee(SKCanvas canvas, float x, float y)
    {
        using var bodyPaint = new SKPaint { Color = new SKColor(255, 215, 0), IsAntialias = true };
        using var stripePaint = new SKPaint { Color = SKColors.Black, IsAntialias = true };
        canvas.DrawOval(x, y, 8, 5, bodyPaint);
        canvas.DrawRect(x - 2, y - 5, 4, 10, stripePaint);
        canvas.DrawRect(x + 3, y - 5, 2, 10, stripePaint);
    }

    private static void DrawRobin(SKCanvas canvas, float x, float y)
    {
        using var bodyPaint = new SKPaint { Color = new SKColor(101, 67, 33), IsAntialias = true };
        using var breastPaint = new SKPaint { Color = new SKColor(220, 50, 30), IsAntialias = true };
        canvas.DrawOval(x, y, 9, 7, bodyPaint);
        canvas.DrawOval(x, y + 2, 5, 4, breastPaint);
        canvas.DrawCircle(x, y - 6, 5, bodyPaint);
    }

    private static void DrawHedgehog(SKCanvas canvas, float x, float y)
    {
        using var bodyPaint = new SKPaint { Color = new SKColor(139, 90, 43), IsAntialias = true };
        using var spikePaint = new SKPaint
        {
            Color = new SKColor(100, 70, 30),
            IsAntialias = true,
            StrokeWidth = 1.5f,
            Style = SKPaintStyle.Stroke
        };
        canvas.DrawOval(x, y, 10, 7, bodyPaint);
        for (int i = -4; i <= 4; i++)
        {
            float sx = x + i * 2;
            canvas.DrawLine(sx, y - 5, sx + i * 0.5f, y - 12, spikePaint);
        }
    }

    private static void DrawFrog(SKCanvas canvas, float x, float y)
    {
        using var paint = new SKPaint { Color = new SKColor(34, 139, 34), IsAntialias = true };
        using var eyePaint = new SKPaint { Color = SKColors.White, IsAntialias = true };
        using var pupilPaint = new SKPaint { Color = SKColors.Black, IsAntialias = true };
        canvas.DrawCircle(x, y, 8, paint);
        canvas.DrawCircle(x - 5, y - 7, 4, eyePaint);
        canvas.DrawCircle(x + 5, y - 7, 4, eyePaint);
        canvas.DrawCircle(x - 5, y - 7, 2, pupilPaint);
        canvas.DrawCircle(x + 5, y - 7, 2, pupilPaint);
    }

    private static void DrawButterfly(SKCanvas canvas, float x, float y)
    {
        using var wingPaint = new SKPaint { Color = new SKColor(255, 140, 0, 200), IsAntialias = true };
        using var bodyPaint = new SKPaint { Color = SKColors.Black, IsAntialias = true };
        canvas.DrawOval(x - 8, y - 4, 7, 5, wingPaint);
        canvas.DrawOval(x + 8, y - 4, 7, 5, wingPaint);
        canvas.DrawOval(x - 6, y + 3, 5, 4, wingPaint);
        canvas.DrawOval(x + 6, y + 3, 5, 4, wingPaint);
        canvas.DrawOval(x, y, 2, 7, bodyPaint);
    }

    #endregion
}
