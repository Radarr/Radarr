using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using FizzWare.NBuilder;
using FizzWare.NBuilder.PropertyNaming;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles.BookImport.Identification;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.BookImport.Identification
{
    // we need to use random strings to test the va (so we don't just get artist1, artist2 etc which are too similar)
    // but the standard random value namer would give paths that are too long on windows
    public class RandomValueNamerShortStrings : RandomValuePropertyNamer
    {
        private static readonly List<char> AllowedChars;
        private readonly IRandomGenerator _generator;

        public RandomValueNamerShortStrings(BuilderSettings settings)
            : base(settings)
        {
            _generator = new RandomGenerator();
        }

        static RandomValueNamerShortStrings()
        {
            AllowedChars = new List<char>();
            for (char c = 'a'; c < 'z'; c++)
            {
                AllowedChars.Add(c);
            }

            for (char c = 'A'; c < 'Z'; c++)
            {
                AllowedChars.Add(c);
            }

            for (char c = '0'; c < '9'; c++)
            {
                AllowedChars.Add(c);
            }
        }

        protected override string GetString(MemberInfo memberInfo)
        {
            int length = _generator.Next(1, 100);

            char[] chars = new char[length];

            for (int i = 0; i < length; i++)
            {
                int index = _generator.Next(0, AllowedChars.Count - 1);
                chars[i] = AllowedChars[index];
            }

            byte[] bytes = Encoding.UTF8.GetBytes(chars);
            return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
        }
    }

    [TestFixture]
    public class TrackGroupingServiceFixture : CoreTest<TrackGroupingService>
    {
        private List<LocalBook> GivenTracks(string root, string artist, string album, int count)
        {
            var fileInfos = Builder<ParsedTrackInfo>
                .CreateListOfSize(count)
                .All()
                .With(f => f.ArtistTitle = artist)
                .With(f => f.AlbumTitle = album)
                .With(f => f.AlbumMBId = null)
                .With(f => f.ReleaseMBId = null)
                .Build();

            var tracks = fileInfos.Select(x => Builder<LocalBook>
                                          .CreateNew()
                                          .With(y => y.FileTrackInfo = x)
                                          .With(y => y.Path = Path.Combine(root, x.Title))
                                          .Build()).ToList();

            return tracks;
        }

        private List<LocalBook> GivenTracksWithNoTags(string root, int count)
        {
            var outp = new List<LocalBook>();

            for (int i = 0; i < count; i++)
            {
                var track = Builder<LocalBook>
                    .CreateNew()
                    .With(y => y.FileTrackInfo = new ParsedTrackInfo())
                    .With(y => y.Path = Path.Combine(root, $"{i}.mp3"))
                    .Build();
                outp.Add(track);
            }

            return outp;
        }

        [Repeat(100)]
        private List<LocalBook> GivenVaTracks(string root, string album, int count)
        {
            var settings = new BuilderSettings();
            settings.SetPropertyNamerFor<ParsedTrackInfo>(new RandomValueNamerShortStrings(settings));

            var builder = new Builder(settings);

            var fileInfos = builder
                .CreateListOfSize<ParsedTrackInfo>(count)
                .All()
                .With(f => f.AlbumTitle = "album")
                .With(f => f.AlbumMBId = null)
                .With(f => f.ReleaseMBId = null)
                .Build();

            var tracks = fileInfos.Select(x => Builder<LocalBook>
                                          .CreateNew()
                                          .With(y => y.FileTrackInfo = x)
                                          .With(y => y.Path = Path.Combine(@"C:\music\incoming".AsOsAgnostic(), x.Title))
                                          .Build()).ToList();

            return tracks;
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(10)]
        public void single_artist_is_not_various_artists(int count)
        {
            var tracks = GivenTracks(@"C:\music\incoming".AsOsAgnostic(), "artist", "album", count);
            TrackGroupingService.IsVariousArtists(tracks).Should().Be(false);
        }

        // GivenVaTracks uses random names so repeat multiple times to try to prompt any intermittent failures
        [Test]
        [Repeat(100)]
        public void all_different_artists_is_various_artists()
        {
            var tracks = GivenVaTracks(@"C:\music\incoming".AsOsAgnostic(), "album", 10);
            TrackGroupingService.IsVariousArtists(tracks).Should().Be(true);
        }

        [Test]
        public void two_artists_is_not_various_artists()
        {
            var dir = @"C:\music\incoming".AsOsAgnostic();
            var tracks = GivenTracks(dir, "artist1", "album", 10);
            tracks.AddRange(GivenTracks(dir, "artist2", "album", 10));

            TrackGroupingService.IsVariousArtists(tracks).Should().Be(false);
        }

        [Test]
        [Repeat(100)]
        public void mostly_different_artists_is_various_artists()
        {
            var dir = @"C:\music\incoming".AsOsAgnostic();
            var tracks = GivenVaTracks(dir, "album", 10);
            tracks.AddRange(GivenTracks(dir, "single_artist", "album", 2));
            TrackGroupingService.IsVariousArtists(tracks).Should().Be(true);
        }

        [TestCase("")]
        [TestCase("Various Artists")]
        [TestCase("Various")]
        [TestCase("VA")]
        [TestCase("Unknown")]
        public void va_artist_title_is_various_artists(string artist)
        {
            var tracks = GivenTracks(@"C:\music\incoming".AsOsAgnostic(), artist, "album", 10);
            TrackGroupingService.IsVariousArtists(tracks).Should().Be(true);
        }

        [TestCase("Va?!")]
        [TestCase("Va Va Voom")]
        [TestCase("V.A. Jr.")]
        [TestCase("Ca Va")]
        public void va_in_artist_name_is_not_various_artists(string artist)
        {
            var tracks = GivenTracks(@"C:\music\incoming".AsOsAgnostic(), artist, "album", 10);
            TrackGroupingService.IsVariousArtists(tracks).Should().Be(false);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(10)]
        public void should_group_single_artist_album(int count)
        {
            var tracks = GivenTracks(@"C:\music\incoming".AsOsAgnostic(), "artist", "album", count);
            var output = Subject.GroupTracks(tracks);

            TrackGroupingService.IsVariousArtists(tracks).Should().Be(false);
            TrackGroupingService.LooksLikeSingleRelease(tracks).Should().Be(true);

            output.Count.Should().Be(1);
            output[0].LocalBooks.Count.Should().Be(count);
        }

        [TestCase("cd")]
        [TestCase("disc")]
        [TestCase("disk")]
        public void should_group_multi_disc_release(string mediaName)
        {
            var tracks = GivenTracks($"C:\\music\\incoming\\artist - album\\{mediaName} 1".AsOsAgnostic(), "artist", "album", 10);
            tracks.AddRange(GivenTracks($"C:\\music\\incoming\\artist - album\\{mediaName} 2".AsOsAgnostic(), "artist", "album", 5));

            TrackGroupingService.IsVariousArtists(tracks).Should().Be(false);
            TrackGroupingService.LooksLikeSingleRelease(tracks).Should().Be(true);

            var output = Subject.GroupTracks(tracks);
            output.Count.Should().Be(1);
            output[0].LocalBooks.Count.Should().Be(15);
        }

        [Test]
        public void should_not_group_two_different_albums_by_same_artist()
        {
            var tracks = GivenTracks($"C:\\music\\incoming\\artist - album1".AsOsAgnostic(), "artist", "album1", 10);
            tracks.AddRange(GivenTracks($"C:\\music\\incoming\\artist - album2".AsOsAgnostic(), "artist", "album2", 5));

            TrackGroupingService.IsVariousArtists(tracks).Should().Be(false);
            TrackGroupingService.LooksLikeSingleRelease(tracks).Should().Be(false);

            var output = Subject.GroupTracks(tracks);
            output.Count.Should().Be(2);
            output[0].LocalBooks.Count.Should().Be(10);
            output[1].LocalBooks.Count.Should().Be(5);
        }

        [Test]
        public void should_group_albums_with_typos()
        {
            var tracks = GivenTracks($"C:\\music\\incoming\\artist - album".AsOsAgnostic(), "artist", "Rastaman Vibration (Remastered)", 10);
            tracks.AddRange(GivenTracks($"C:\\music\\incoming\\artist - album".AsOsAgnostic(), "artist", "Rastaman Vibration (Remastered", 5));

            TrackGroupingService.IsVariousArtists(tracks).Should().Be(false);
            TrackGroupingService.LooksLikeSingleRelease(tracks).Should().Be(true);

            var output = Subject.GroupTracks(tracks);
            output.Count.Should().Be(1);
            output[0].LocalBooks.Count.Should().Be(15);
        }

        [Test]
        public void should_not_group_two_different_tracks_in_same_directory()
        {
            var tracks = GivenTracks($"C:\\music\\incoming".AsOsAgnostic(), "artist", "album1", 1);
            tracks.AddRange(GivenTracks($"C:\\music\\incoming".AsOsAgnostic(), "artist", "album2", 1));

            TrackGroupingService.IsVariousArtists(tracks).Should().Be(false);
            TrackGroupingService.LooksLikeSingleRelease(tracks).Should().Be(false);

            var output = Subject.GroupTracks(tracks);
            output.Count.Should().Be(2);
            output[0].LocalBooks.Count.Should().Be(1);
            output[1].LocalBooks.Count.Should().Be(1);
        }

        [Test]
        public void should_separate_two_albums_in_same_directory()
        {
            var tracks = GivenTracks($"C:\\music\\incoming\\artist discog".AsOsAgnostic(), "artist", "album1", 10);
            tracks.AddRange(GivenTracks($"C:\\music\\incoming\\artist disog".AsOsAgnostic(), "artist", "album2", 5));

            TrackGroupingService.IsVariousArtists(tracks).Should().Be(false);
            TrackGroupingService.LooksLikeSingleRelease(tracks).Should().Be(false);

            var output = Subject.GroupTracks(tracks);
            output.Count.Should().Be(2);
            output[0].LocalBooks.Count.Should().Be(10);
            output[1].LocalBooks.Count.Should().Be(5);
        }

        [Test]
        public void should_separate_many_albums_in_same_directory()
        {
            var tracks = new List<LocalBook>();
            for (int i = 0; i < 100; i++)
            {
                tracks.AddRange(GivenTracks($"C:\\music".AsOsAgnostic(), "artist" + i, "album" + i, 10));
            }

            // don't test various artists here because it's designed to only work if there's a common album
            TrackGroupingService.LooksLikeSingleRelease(tracks).Should().Be(false);

            var output = Subject.GroupTracks(tracks);
            output.Count.Should().Be(100);
            output.Select(x => x.LocalBooks.Count).Distinct().Should().BeEquivalentTo(new List<int> { 10 });
        }

        [Test]
        public void should_separate_two_albums_by_different_artists_in_same_directory()
        {
            var tracks = GivenTracks($"C:\\music\\incoming".AsOsAgnostic(), "artist1", "album1", 10);
            tracks.AddRange(GivenTracks($"C:\\music\\incoming".AsOsAgnostic(), "artist2", "album2", 5));

            TrackGroupingService.IsVariousArtists(tracks).Should().Be(false);
            TrackGroupingService.LooksLikeSingleRelease(tracks).Should().Be(false);

            var output = Subject.GroupTracks(tracks);
            output.Count.Should().Be(2);
            output[0].LocalBooks.Count.Should().Be(10);
            output[1].LocalBooks.Count.Should().Be(5);
        }

        [Test]
        [Repeat(100)]
        public void should_group_va_release()
        {
            var tracks = GivenVaTracks(@"C:\music\incoming".AsOsAgnostic(), "album", 10);

            TrackGroupingService.IsVariousArtists(tracks).Should().Be(true);
            TrackGroupingService.LooksLikeSingleRelease(tracks).Should().Be(true);

            var output = Subject.GroupTracks(tracks);
            output.Count.Should().Be(1);
            output[0].LocalBooks.Count.Should().Be(10);
        }

        [Test]
        public void should_not_group_two_albums_by_different_artists_with_same_title()
        {
            var tracks = GivenTracks($"C:\\music\\incoming\\album".AsOsAgnostic(), "artist1", "album", 10);
            tracks.AddRange(GivenTracks($"C:\\music\\incoming\\album".AsOsAgnostic(), "artist2", "album", 5));

            TrackGroupingService.IsVariousArtists(tracks).Should().Be(false);
            TrackGroupingService.LooksLikeSingleRelease(tracks).Should().Be(false);

            var output = Subject.GroupTracks(tracks);

            output.Count.Should().Be(2);
            output[0].LocalBooks.Count.Should().Be(10);
            output[1].LocalBooks.Count.Should().Be(5);
        }

        [Test]
        public void should_not_fail_if_all_tags_null()
        {
            var tracks = GivenTracksWithNoTags($"C:\\music\\incoming\\album".AsOsAgnostic(), 10);

            TrackGroupingService.IsVariousArtists(tracks).Should().Be(false);
            TrackGroupingService.LooksLikeSingleRelease(tracks).Should().Be(true);

            var output = Subject.GroupTracks(tracks);
            output.Count.Should().Be(1);
            output[0].LocalBooks.Count.Should().Be(10);
        }

        [Test]
        public void should_not_fail_if_some_tags_null()
        {
            var tracks = GivenTracks($"C:\\music\\incoming\\album".AsOsAgnostic(), "artist1", "album", 10);
            tracks.AddRange(GivenTracksWithNoTags($"C:\\music\\incoming\\album".AsOsAgnostic(), 2));

            TrackGroupingService.IsVariousArtists(tracks).Should().Be(false);
            TrackGroupingService.LooksLikeSingleRelease(tracks).Should().Be(true);

            var output = Subject.GroupTracks(tracks);
            output.Count.Should().Be(1);
            output[0].LocalBooks.Count.Should().Be(12);
        }

        [Test]
        public void should_cope_with_one_album_in_subfolder_of_another()
        {
            var tracks = GivenTracks($"C:\\music\\incoming\\album".AsOsAgnostic(), "artist1", "album", 10);
            tracks.AddRange(GivenTracks($"C:\\music\\incoming\\album\\anotheralbum".AsOsAgnostic(), "artist2", "album2", 10));

            TrackGroupingService.IsVariousArtists(tracks).Should().Be(false);
            TrackGroupingService.LooksLikeSingleRelease(tracks).Should().Be(false);

            var output = Subject.GroupTracks(tracks);

            foreach (var group in output)
            {
                TestLogger.Debug($"*** group {group} ***");
                TestLogger.Debug(string.Join("\n", group.LocalBooks.Select(x => x.Path)));
            }

            output.Count.Should().Be(2);
            output[0].LocalBooks.Count.Should().Be(10);
            output[1].LocalBooks.Count.Should().Be(10);
        }
    }
}
