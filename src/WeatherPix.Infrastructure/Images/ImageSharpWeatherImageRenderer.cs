using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using WeatherPix.Application.Abstractions;
using WeatherPix.Domain.WeatherStation;

namespace WeatherPix.Infrastructure.Images;

public sealed class ImageSharpWeatherImageRenderer : IWeatherImageRenderer
{
    private const int ImageWidth = 1200;
    private const int ImageHeight = 800;

    private const int CardX = 20;
    private const int CardY = 20;
    private const int CardWidth = 560;
    private const int CardHeight = 430;

    private const float TitleX = 40;
    private const float TitleY = 40;

    private const float LabelX = 40;
    private const float ValueX = 185;
    private const float FirstRowY = 155;
    private const float RowHeight = 31;

    public async Task<Stream> RenderAsync(
        Stream sourceImage,
        WeatherStation station,
        CancellationToken ct)
    {
        using var image = await Image.LoadAsync(sourceImage, ct);

        var titleColor = Color.ParseHex("#2D2D2D");
        var bodyColor = Color.ParseHex("#414141");

        var titleFont = SystemFonts.CreateFont("Arial", 34, FontStyle.Bold);
        var subtitleFont = SystemFonts.CreateFont("Arial", 28, FontStyle.Bold);
        var bodyFont = SystemFonts.CreateFont("Arial", 20, FontStyle.Regular);

        image.Mutate(ctx =>
        {
            ctx.Resize(new ResizeOptions
            {
                Size = new Size(ImageWidth, ImageHeight),
                Mode = ResizeMode.Crop
            });

            ctx.Paint(canvas =>
            {
                canvas.Fill(
                    Brushes.Solid(Color.White.WithAlpha(0.2f)),
                    new Rectangle(CardX, CardY, CardWidth, CardHeight));

                DrawText(
                    canvas,
                    $"{station.Name ?? "Unknown station"}",
                    titleFont,
                    titleColor,
                    TitleX,
                    TitleY);

                DrawText(
                    canvas,
                    $"{station.Region ?? "Unknown region"}",
                    subtitleFont,
                    titleColor,
                    TitleX,
                    TitleY + 45);

                var currentY = FirstRowY;

                DrawDetail(canvas, "Temperature", FormatValue(station.TemperatureCelsius, "°C"), bodyFont, bodyColor, currentY);
                currentY += RowHeight;

                DrawDetail(canvas, "Feels like", FormatValue(station.FeelTemperatureCelsius, "°C"), bodyFont, bodyColor, currentY);
                currentY += RowHeight;

                DrawDetail(canvas, "Humidity", FormatValue(station.HumidityPercentage, "%"), bodyFont, bodyColor, currentY);
                currentY += RowHeight;

                DrawDetail(canvas, "Wind", FormatWind(station.WindSpeedMetersPerSecond, station.WindDirection), bodyFont, bodyColor, currentY);
                currentY += RowHeight;

                DrawDetail(canvas, "Wind gusts", FormatValue(station.WindGustMetersPerSecond, "m/s"), bodyFont, bodyColor, currentY);
                currentY += RowHeight;

                DrawDetail(canvas, "Pressure", FormatValue(station.AirPressure, "hPa"), bodyFont, bodyColor, currentY);
                currentY += RowHeight;

                DrawDetail(canvas, "Visibility", FormatVisibility(station.VisibilityMeters), bodyFont, bodyColor, currentY);
                currentY += RowHeight;

                DrawDetail(canvas, "Measured", station.MeasuredAt.ToString("dd-MM-yyyy HH:mm"), bodyFont, bodyColor, currentY);
            });
        });

        var output = new MemoryStream();

        await image.SaveAsync(
            output,
            new JpegEncoder
            {
                Quality = 90
            },
            ct);

        output.Position = 0;

        return output;
    }

    private static void DrawDetail(
        DrawingCanvas canvas,
        string label,
        string value,
        Font font,
        Color color,
        float y)
    {
        DrawText(canvas, label, font, color, LabelX, y);
        DrawText(canvas, value, font, color, ValueX, y);
    }

    private static void DrawText(
        DrawingCanvas canvas,
        string text,
        Font font,
        Color color,
        float x,
        float y)
    {
        var textOptions = new RichTextOptions(font)
        {
            Origin = new PointF(x, y)
        };

        canvas.DrawText(
            textOptions,
            text,
            Brushes.Solid(color),
            pen: null);
    }

    private static string FormatValue(double? value, string unit)
    {
        return value.HasValue
            ? $"{value.Value:0.#} {unit}"
            : "N/A";
    }

    private static string FormatVisibility(double? meters)
    {
        if (!meters.HasValue)
        {
            return "N/A";
        }

        return meters.Value >= 1000
            ? $"{meters.Value / 1000:0.#} km"
            : $"{meters.Value:0} m";
    }

    private static string FormatWind(double? speed, string? direction)
    {
        var speedText = FormatValue(speed, "m/s");

        return string.IsNullOrWhiteSpace(direction)
            ? speedText
            : $"{speedText} {direction}";
    }
}