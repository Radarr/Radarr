using NUnit.Framework;
using NzbDrone.Core.MediaFiles.TrackImport.Identification;
using FluentAssertions;
using NzbDrone.Core.Test.Framework;
using FizzWare.NBuilder;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Music;
using System.Collections.Generic;
using System.Linq;
using System;
using NzbDrone.Core.Parser;
using NzbDrone.Common.Serializer;

namespace NzbDrone.Core.Test.MediaFiles.TrackImport.Identification
{
    [TestFixture]
    public class TrackMappingFixture : CoreTest<IdentificationService>
    {

        private ArtistMetadata artist;

        [SetUp]
        public void Setup()
        {
            artist = Builder<ArtistMetadata>
                .CreateNew()
                .With(x => x.Name = "artist")
                .Build();
        }
        
        private List<Track> GivenTracks(int count)
        {
             return Builder<Track>
                .CreateListOfSize(count)
                .All()
                .With(x => x.ArtistMetadata = artist)
                .Build()
                .ToList();
        }

        private ParsedTrackInfo GivenParsedTrackInfo(Track track, AlbumRelease release)
        {
            return Builder<ParsedTrackInfo>
                .CreateNew()
                .With(x => x.Title = track.Title)
                .With(x => x.CleanTitle = track.Title.CleanTrackTitle())
                .With(x => x.AlbumTitle = release.Title)
                .With(x => x.Disambiguation = release.Disambiguation)
                .With(x => x.ReleaseMBId = release.ForeignReleaseId)
                .With(x => x.ArtistTitle = track.ArtistMetadata.Value.Name)
                .With(x => x.TrackNumbers = new[] { track.AbsoluteTrackNumber })
                .With(x => x.RecordingMBId = track.ForeignRecordingId)
                .With(x => x.Country = IsoCountries.Find("US"))
                .With(x => x.Label = release.Label.First())
                .With(x => x.Year = (uint)release.Album.Value.ReleaseDate.Value.Year)
                .Build();
        }

        private List<LocalTrack> GivenLocalTracks(List<Track> tracks, AlbumRelease release)
        {
            var output = Builder<LocalTrack>
                .CreateListOfSize(tracks.Count)
                .Build()
                .ToList();

            for (int i = 0; i < tracks.Count; i++)
            {
                output[i].FileTrackInfo = GivenParsedTrackInfo(tracks[i], release);
            }

            return output;
        }

        private AlbumRelease GivenAlbumRelease(string title, List<Track> tracks)
        {
            var album = Builder<Album>
                .CreateNew()
                .With(x => x.Title = title)
                .With(x => x.ArtistMetadata = artist)
                .Build();

            var media = Builder<Medium>
                .CreateListOfSize(1)
                .Build()
                .ToList();

            return Builder<AlbumRelease>
                .CreateNew()
                .With(x => x.Tracks = tracks)
                .With(x => x.Title = title)
                .With(x => x.Album = album)
                .With(x => x.Media = media)
                .With(x => x.Country = new List<string>())
                .With(x => x.Label = new List<string> { "label" })
                .Build();
        }

        [Test]
        public void test_reorder_when_track_numbers_incorrect()
        {
            var tracks = GivenTracks(3);
            var release = GivenAlbumRelease("album", tracks);
            var localTracks = GivenLocalTracks(tracks, release);

            localTracks[2].FileTrackInfo.TrackNumbers = new [] { 2 };
            localTracks[1].FileTrackInfo.TrackNumbers = new [] { 3 };
            localTracks = new [] {0, 2, 1}.Select(x => localTracks[x]).ToList();

            var result = Subject.MapReleaseTracks(localTracks, tracks);
            
            result.Mapping
                .ToDictionary(x => x.Key, y => y.Value.Item1)
                .Should().BeEquivalentTo(new Dictionary<LocalTrack, Track> {
                        {localTracks[0], tracks[0]},
                        {localTracks[1], tracks[2]},
                        {localTracks[2], tracks[1]},
                    });
            result.LocalExtra.Should().BeEmpty();
            result.MBExtra.Should().BeEmpty();
        }

        [Test]
        public void test_order_works_with_invalid_track_numbers()
        {
            var tracks = GivenTracks(3);
            var release = GivenAlbumRelease("album", tracks);
            var localTracks = GivenLocalTracks(tracks, release);

            foreach (var track in localTracks)
            {
                track.FileTrackInfo.TrackNumbers = new[] { 1 };
            }

            var result = Subject.MapReleaseTracks(localTracks, tracks);

            result.Mapping
                .ToDictionary(x => x.Key, y => y.Value.Item1)
                .Should().BeEquivalentTo(new Dictionary<LocalTrack, Track> {
                        {localTracks[0], tracks[0]},
                        {localTracks[1], tracks[1]},
                        {localTracks[2], tracks[2]},
                    });
            result.LocalExtra.Should().BeEmpty();
            result.MBExtra.Should().BeEmpty();
        }

        [Test]
        public void test_order_works_with_missing_tracks()
        {
            var tracks = GivenTracks(3);
            var release = GivenAlbumRelease("album", tracks);
            var localTracks = GivenLocalTracks(tracks, release);
            localTracks.RemoveAt(1);

            var result = Subject.MapReleaseTracks(localTracks, tracks);
            
            result.Mapping
                .ToDictionary(x => x.Key, y => y.Value.Item1)
                .Should().BeEquivalentTo(new Dictionary<LocalTrack, Track> {
                        {localTracks[0], tracks[0]},
                        {localTracks[1], tracks[2]}
                    });
            result.LocalExtra.Should().BeEmpty();
            result.MBExtra.Should().BeEquivalentTo(new List<Track> { tracks[1] });
        }

        [Test]
        public void test_order_works_with_extra_tracks()
        {
            var tracks = GivenTracks(3);
            var release = GivenAlbumRelease("album", tracks);
            var localTracks = GivenLocalTracks(tracks, release);
            tracks.RemoveAt(1);

            var result = Subject.MapReleaseTracks(localTracks, tracks);
            
            result.Mapping
                .ToDictionary(x => x.Key, y => y.Value.Item1)
                .Should().BeEquivalentTo(new Dictionary<LocalTrack, Track> {
                        {localTracks[0], tracks[0]},
                        {localTracks[2], tracks[1]}
                    });
            result.LocalExtra.Should().BeEquivalentTo(new List<LocalTrack> { localTracks[1] });
            result.MBExtra.Should().BeEmpty();
        }
    }
}
