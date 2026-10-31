using System.IO;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.Test.Framework;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace NzbDrone.Core.Test.MediaCoverTests
{
    [TestFixture]
    public class PosterRendererFixture : CoreTest<PosterRenderer>
    {
        private string _destination;

        [SetUp]
        public void Setup()
        {
            _destination = Path.Combine(TempFolder, "poster.jpg");
        }

        [TestCase("Alien")]
        [TestCase("The Texas Chain Saw Massacre")]
        [TestCase("Dr. Strangelove or: How I Learned to Stop Worrying and Love the Bomb")]
        [TestCase("Supercalifragilisticexpialidociousandthensomeevenlongerwordthatcannotwrap")]
        [TestCase("リング")]
        [TestCase("")]
        public void should_render_poster(string title)
        {
            Subject.Render(new PosterReplacement(title, "#1c1c1c", "#ffffff"), _destination);

            using var image = Image.Load<Rgb24>(_destination);

            image.Width.Should().Be(PosterRenderer.Width);
            image.Height.Should().Be(PosterRenderer.Height);
            File.Exists(_destination + ".tmp").Should().BeFalse();
        }

        [Test]
        public void should_use_background_color_and_draw_title_in_text_color()
        {
            Subject.Render(new PosterReplacement("Halloween", "#550000", "#ffffff"), _destination);

            using var image = Image.Load<Rgb24>(_destination);

            // Corners are background, allowing for JPEG artifacts
            var corner = image[5, 5];
            corner.R.Should().BeInRange(0x50, 0x5a);
            corner.G.Should().BeLessThan(8);
            corner.B.Should().BeLessThan(8);

            var hasText = false;

            image.ProcessPixelRows(accessor =>
            {
                var row = accessor.GetRowSpan(PosterRenderer.Height / 2);

                foreach (var pixel in row)
                {
                    hasText |= pixel.G > 200 && pixel.B > 200;
                }
            });

            hasText.Should().BeTrue();
        }

        [Test]
        public void should_overwrite_existing_poster()
        {
            File.WriteAllText(_destination, "original");

            Subject.Render(new PosterReplacement("Halloween", "#000000", "#ffffff"), _destination);

            using var image = Image.Load<Rgb24>(_destination);
            image.Width.Should().Be(PosterRenderer.Width);
        }
    }
}
