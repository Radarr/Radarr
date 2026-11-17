using System;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace NzbDrone.Core.MediaCover
{
    public interface IPosterRenderer
    {
        void Render(PosterReplacement replacement, string destination);
    }

    public class PosterRenderer : IPosterRenderer
    {
        // 2:3, the aspect ratio of TMDb posters
        public const int Width = 1000;
        public const int Height = 1500;

        private const float MaxFontSize = 150;
        private const float MinFontSize = 36;
        private const float HorizontalPadding = 0.1f;
        private const float VerticalPadding = 0.15f;

        private static readonly Lazy<FontFamily> PrimaryFontFamily = new(LoadPrimaryFontFamily);
        private static readonly Lazy<FontFamily[]> FallbackFontFamilies = new(LoadFallbackFontFamilies);

        private readonly Logger _logger;

        public PosterRenderer(Logger logger)
        {
            _logger = logger;
        }

        public void Render(PosterReplacement replacement, string destination)
        {
            var background = Color.ParseHex(replacement.BackgroundColor);
            var foreground = Color.ParseHex(replacement.TextColor);

            using var image = new Image<Rgb24>(Width, Height, background.ToPixel<Rgb24>());

            var title = replacement.Title.Trim();

            if (title.IsNotNullOrWhiteSpace())
            {
                var options = GetTextOptions(title);

                image.Mutate(x => x.DrawText(options, title, foreground));
            }

            var tempPath = destination + ".tmp";

            try
            {
                // Write to a temporary file first so a failed render never leaves a partial poster behind
                image.SaveAsJpeg(tempPath);
                File.Move(tempPath, destination, true);
            }
            catch
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }

                throw;
            }
        }

        private RichTextOptions GetTextOptions(string title)
        {
            var maxWidth = Width * (1 - (2 * HorizontalPadding));
            var maxHeight = Height * (1 - (2 * VerticalPadding));

            RichTextOptions options = null;

            // Shrink the font until the wrapped title fits. Words never break, so the widest word must fit as well.
            for (var size = MaxFontSize; size >= MinFontSize; size -= 6)
            {
                options = CreateTextOptions(size, maxWidth);

                var bounds = TextMeasurer.MeasureBounds(title, options);

                if (bounds.Width <= maxWidth && bounds.Height <= maxHeight)
                {
                    return options;
                }
            }

            _logger.Trace("Title '{0}' does not fit on poster at minimum font size, it will be clipped", title);

            return CreateTextOptions(MinFontSize, maxWidth);
        }

        private static RichTextOptions CreateTextOptions(float size, float wrappingLength)
        {
            return new RichTextOptions(PrimaryFontFamily.Value.CreateFont(size, FontStyle.Regular))
            {
                FallbackFontFamilies = FallbackFontFamilies.Value,
                Origin = new PointF(Width / 2f, Height / 2f),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                WrappingLength = wrappingLength,
                LineSpacing = 1.15f
            };
        }

        private static FontFamily LoadPrimaryFontFamily()
        {
            using var stream = typeof(PosterRenderer).Assembly.GetManifestResourceStream("NzbDrone.Core.Resources.Fonts.Roboto-Regular.ttf");

            var collection = new FontCollection();

            return collection.Add(stream);
        }

        // System fonts cover scripts Roboto lacks (CJK, Arabic, ...) when they're available
        private static FontFamily[] LoadFallbackFontFamilies()
        {
            try
            {
                return SystemFonts.Families.ToArray();
            }
            catch
            {
                return Array.Empty<FontFamily>();
            }
        }
    }
}
