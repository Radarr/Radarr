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
using Moq;

namespace NzbDrone.Core.Test.MediaFiles.TrackImport.Identification
{
    [TestFixture]
    public class GetCandidatesFixture : CoreTest<IdentificationService>
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
                .With(x => x.ForeignReleaseId = null)
                .Build();
        }

        private LocalAlbumRelease GivenLocalAlbumRelease()
        {
            var tracks = GivenTracks(3);
            var release = GivenAlbumRelease("album", tracks);
            var localTracks = GivenLocalTracks(tracks, release);

            return new LocalAlbumRelease(localTracks);
        }

        [Test]
        public void get_candidates_by_fingerprint_should_not_fail_if_fingerprint_lookup_returned_null()
        {
            Mocker.GetMock<IFingerprintingService>()
                .Setup(x => x.Lookup(It.IsAny<List<LocalTrack>>(), It.IsAny<double>()))
                .Callback((List<LocalTrack> x, double thres) => {
                        foreach(var track in x) {
                            track.AcoustIdResults = null;
                        }
                    });

            Mocker.GetMock<IReleaseService>()
                .Setup(x => x.GetReleasesByRecordingIds(It.IsAny<List<string>>()))
                .Returns(new List<AlbumRelease>());

            var local = GivenLocalAlbumRelease();

            Subject.GetCandidatesFromFingerprint(local, null, null, null, false).ShouldBeEquivalentTo(new List<CandidateAlbumRelease>());
        }

        [Test]
        public void get_candidates_should_only_return_specified_release_if_set()
        {
            var tracks = GivenTracks(3);
            var release = GivenAlbumRelease("album", tracks);
            var localTracks = GivenLocalTracks(tracks, release);
            var localAlbumRelease = new LocalAlbumRelease(localTracks);

            Subject.GetCandidatesFromTags(localAlbumRelease, null, null, release, false).ShouldBeEquivalentTo(
                new List<CandidateAlbumRelease> { new CandidateAlbumRelease(release) }
                );
        }

        [Test]
        public void get_candidates_should_use_consensus_release_id()
        {
            var tracks = GivenTracks(3);
            var release = GivenAlbumRelease("album", tracks);
            release.ForeignReleaseId = "xxx";
            var localTracks = GivenLocalTracks(tracks, release);
            var localAlbumRelease = new LocalAlbumRelease(localTracks);

            Mocker.GetMock<IReleaseService>()
                  .Setup(x => x.GetReleaseByForeignReleaseId("xxx", true))
                  .Returns(release);

            Subject.GetCandidatesFromTags(localAlbumRelease, null, null, null, false).ShouldBeEquivalentTo(
                new List<CandidateAlbumRelease> { new CandidateAlbumRelease(release) }
                );
        }
    }
}
