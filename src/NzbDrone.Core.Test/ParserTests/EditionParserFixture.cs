using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class EditionParserFixture : CoreTest
    {
        [TestCase("Prometheus 2012 Directors Cut", "Directors Cut")]
        [TestCase("Star Wars Episode IV - A New Hope 1999 (Despecialized).mkv", "Despecialized")]
        [TestCase("Prometheus.2012.(Special.Edition.Remastered).[Bluray-1080p].mkv", "Special Edition Remastered")]
        [TestCase("Prometheus 2012 Extended", "Extended")]
        [TestCase("Prometheus 2012 Extended Directors Cut Fan Edit", "Extended Directors Cut Fan Edit")]
        [TestCase("Prometheus 2012 Director's Cut", "Director's Cut")]
        [TestCase("Prometheus 2012 Directors Cut", "Directors Cut")]
        [TestCase("Prometheus.2012.(Extended.Theatrical.Version.IMAX).BluRay.1080p.2012.asdf", "Extended Theatrical Version IMAX")]
        [TestCase("2001 A Space Odyssey (1968) Director's Cut .mkv", "Director's Cut")]
        [TestCase("2001: A Space Odyssey 1968 (Extended Directors Cut FanEdit)", "Extended Directors Cut FanEdit")]
        [TestCase("A Fake Movie 2035 2012 Directors.mkv", "Directors")]
        [TestCase("Blade Runner 2049 Director's Cut.mkv", "Director's Cut")]
        [TestCase("Prometheus 2012 50th Anniversary Edition.mkv", "50th Anniversary Edition")]
        [TestCase("Movie 2012 2in1.mkv", "2in1")]
        [TestCase("Movie 2012 IMAX.mkv", "IMAX")]
        [TestCase("Movie 2012 Restored.mkv", "Restored")]
        [TestCase("Prometheus.Special.Edition.Fan Edit.2012..BRRip.x264.AAC-m2g", "Special Edition Fan Edit")]
        [TestCase("Star Wars Episode IV - A New Hope (Despecialized) 1999.mkv", "Despecialized")]
        [TestCase("Prometheus.(Special.Edition.Remastered).2012.[Bluray-1080p].mkv", "Special Edition Remastered")]
        [TestCase("Prometheus Extended 2012", "Extended")]
        [TestCase("Prometheus Extended Directors Cut Fan Edit 2012", "Extended Directors Cut Fan Edit")]
        [TestCase("Prometheus Director's Cut 2012", "Director's Cut")]
        [TestCase("Prometheus Directors Cut 2012", "Directors Cut")]
        [TestCase("Prometheus.(Extended.Theatrical.Version.IMAX).2012.BluRay.1080p.asdf", "Extended Theatrical Version IMAX")]
        [TestCase("2001 A Space Odyssey Director's Cut (1968).mkv", "Director's Cut")]
        [TestCase("2001: A Space Odyssey (Extended Directors Cut FanEdit) 1968 Bluray 1080p", "Extended Directors Cut FanEdit")]
        [TestCase("A Fake Movie 2035 Directors 2012.mkv", "Directors")]
        [TestCase("Blade Runner Director's Cut 2049.mkv", "Director's Cut")]
        [TestCase("Prometheus 50th Anniversary Edition 2012.mkv", "50th Anniversary Edition")]
        [TestCase("Movie 2in1 2012.mkv", "2in1")]
        [TestCase("Movie IMAX 2012.mkv", "IMAX")]
        [TestCase("Fake Movie Final Cut 2016", "Final Cut")]
        [TestCase("Fake Movie 2016 Final Cut ", "Final Cut")]
        [TestCase("My Movie GERMAN Extended Cut 2016", "Extended Cut")]
        [TestCase("My.Movie.GERMAN.Extended.Cut.2016", "Extended Cut")]
        [TestCase("My.Movie.GERMAN.Extended.Cut", "Extended Cut")]
        [TestCase("My.Movie.Assembly.Cut.1992.REPACK.1080p.BluRay.DD5.1.x264-Group", "Assembly Cut")]
        public void should_parse_edition(string postTitle, string edition)
        {
            var parsed = Parser.Parser.ParseMovieTitle(postTitle);
            parsed.Edition.Should().Be(edition);
        }

        [TestCase("Star.Wars.Holiday.Special.1978.DVD.REMUX.DD.2.0-ViETNAM")]
        [TestCase("Directors.Cut.German.2006.COMPLETE.PAL.DVDR-LoD")]
        [TestCase("Mission Impossible: Rogue Nation 2012 Bluray")]
        [TestCase("Loving.Pablo.2018.TS.FRENCH.MD.x264-DROGUERiE")]
        [TestCase("Uncut.Gems.2019.720p.BluRay.x264-YOL0W")]
        public void should_not_parse_edition(string postTitle)
        {
            var parsed = Parser.Parser.ParseMovieTitle(postTitle);
            parsed.Edition.Should().Be("");
        }
    }
}
