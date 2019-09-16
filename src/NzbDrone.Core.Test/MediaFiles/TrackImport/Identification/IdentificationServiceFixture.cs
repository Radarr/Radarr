using System.IO;
using System.Linq;
using System.Collections;
using FluentAssertions;
using FluentValidation.Results;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles.TrackImport.Identification;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.MetadataSource.SkyHook;
using NzbDrone.Core.Music;
using NzbDrone.Core.Music.Commands;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Profiles.Metadata;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;
using System.Collections.Generic;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Parser;
using NzbDrone.Core.MediaFiles.TrackImport.Aggregation.Aggregators;
using NzbDrone.Core.MediaFiles.TrackImport.Aggregation;

namespace NzbDrone.Core.Test.MediaFiles.TrackImport.Identification
{
    [TestFixture]
    public class IdentificationServiceFixture : DbTest
    {
        private ArtistService _artistService;
        private AddArtistService _addArtistService;
        private RefreshArtistService _refreshArtistService;

        private IdentificationService Subject;
        
        [SetUp]
        public void SetUp()
        {
            UseRealHttp();
            
            // Resolve all the parts we need
            Mocker.SetConstant<IArtistRepository>(Mocker.Resolve<ArtistRepository>());
            Mocker.SetConstant<IArtistMetadataRepository>(Mocker.Resolve<ArtistMetadataRepository>());
            Mocker.SetConstant<IAlbumRepository>(Mocker.Resolve<AlbumRepository>());
            Mocker.SetConstant<IReleaseRepository>(Mocker.Resolve<ReleaseRepository>());
            Mocker.SetConstant<ITrackRepository>(Mocker.Resolve<TrackRepository>());

            Mocker.GetMock<IMetadataProfileService>().Setup(x => x.Exists(It.IsAny<int>())).Returns(true);

            _artistService = Mocker.Resolve<ArtistService>();
            Mocker.SetConstant<IArtistService>(_artistService);
            Mocker.SetConstant<IArtistMetadataService>(Mocker.Resolve<ArtistMetadataService>());
            Mocker.SetConstant<IAlbumService>(Mocker.Resolve<AlbumService>());
            Mocker.SetConstant<IReleaseService>(Mocker.Resolve<ReleaseService>());
            Mocker.SetConstant<ITrackService>(Mocker.Resolve<TrackService>());

            Mocker.SetConstant<IConfigService>(Mocker.Resolve<IConfigService>());
            Mocker.SetConstant<IProvideArtistInfo>(Mocker.Resolve<SkyHookProxy>());
            Mocker.SetConstant<IProvideAlbumInfo>(Mocker.Resolve<SkyHookProxy>());
            
            _addArtistService = Mocker.Resolve<AddArtistService>();

            Mocker.SetConstant<IRefreshTrackService>(Mocker.Resolve<RefreshTrackService>());
            Mocker.SetConstant<IRefreshAlbumReleaseService>(Mocker.Resolve<RefreshAlbumReleaseService>());
            Mocker.SetConstant<IRefreshAlbumService>(Mocker.Resolve<RefreshAlbumService>());
            _refreshArtistService = Mocker.Resolve<RefreshArtistService>();

            Mocker.GetMock<IAddArtistValidator>().Setup(x => x.Validate(It.IsAny<Artist>())).Returns(new ValidationResult());

            Mocker.SetConstant<ITrackGroupingService>(Mocker.Resolve<TrackGroupingService>());

            // set up the augmenters
            List<IAggregate<LocalAlbumRelease>> aggregators = new List<IAggregate<LocalAlbumRelease>> {
                Mocker.Resolve<AggregateFilenameInfo>()
            };
            Mocker.SetConstant<IEnumerable<IAggregate<LocalAlbumRelease>>>(aggregators);
            Mocker.SetConstant<IAugmentingService>(Mocker.Resolve<AugmentingService>());
            
            Subject = Mocker.Resolve<IdentificationService>();

        }

        private void GivenMetadataProfile(MetadataProfile profile)
        {
            Mocker.GetMock<IMetadataProfileService>().Setup(x => x.Get(profile.Id)).Returns(profile);
        }

        private List<Artist> GivenArtists(List<ArtistTestCase> artists)
        {
            var outp = new List<Artist>();
            for (int i = 0; i < artists.Count; i++)
            {
                var meta = artists[i].MetadataProfile;
                meta.Id = i + 1;
                GivenMetadataProfile(meta);
                outp.Add(GivenArtist(artists[i].Artist, meta.Id));
            }

            return outp;
        }

        private Artist GivenArtist(string foreignArtistId, int metadataProfileId)
        {
            var artist = _addArtistService.AddArtist(new Artist {
                    Metadata = new ArtistMetadata {
                        ForeignArtistId = foreignArtistId
                    },
                    Path = @"c:\test".AsOsAgnostic(),
                    MetadataProfileId = metadataProfileId
                });

            var command = new RefreshArtistCommand{
                ArtistId = artist.Id,
                Trigger = CommandTrigger.Unspecified
            };

            _refreshArtistService.Execute(command);

            return _artistService.FindById(foreignArtistId);
        }

        private void GivenFingerprints(List<AcoustIdTestCase> fingerprints)
        {
            Mocker.GetMock<IConfigService>().Setup(x => x.AllowFingerprinting).Returns(AllowFingerprinting.AllFiles);
            Mocker.GetMock<IFingerprintingService>().Setup(x => x.IsSetup()).Returns(true);

            Mocker.GetMock<IFingerprintingService>()
                .Setup(x => x.Lookup(It.IsAny<List<LocalTrack>>(), It.IsAny<double>()))
                .Callback((List<LocalTrack> track, double thres) => {
                        track.ForEach(x => x.AcoustIdResults = fingerprints.SingleOrDefault(f => f.Path == x.Path).AcoustIdResults);
                    });
        }

        public static class IdTestCaseFactory
        {
            // for some reason using Directory.GetFiles causes nUnit to error
            private static string[] files = {
                "FilesWithMBIds.json",
                "PreferMissingToBadMatch.json",
                "InconsistentTyposInAlbum.json",
                "SucceedWhenManyAlbumsHaveSameTitle.json",
                "PenalizeUnknownMedia.json",
                "CorruptFile.json",
                "FilesWithoutTags.json"
            };

            public static IEnumerable TestCases
            {
                get
                {
                    foreach (var file in files)
                    {
                        yield return new TestCaseData(file).SetName($"should_match_tracks_{file.Replace(".json", "")}");
                    }
                }
            }
        }

        // these are slow to run so only do so manually
        [Explicit]
        [Test, TestCaseSource(typeof(IdTestCaseFactory), "TestCases")]
        public void should_match_tracks(string file)
        {
            var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "Files", "Identification", file);
            var testcase = JsonConvert.DeserializeObject<IdTestCase>(File.ReadAllText(path));

            var artists = GivenArtists(testcase.LibraryArtists);
            var specifiedArtist = artists.SingleOrDefault(x => x.Metadata.Value.ForeignArtistId == testcase.Artist);

            var tracks = testcase.Tracks.Select(x => new LocalTrack {
                    Path = x.Path.AsOsAgnostic(),
                    FileTrackInfo = x.FileTrackInfo
                }).ToList();

            if (testcase.Fingerprints != null)
            {
                GivenFingerprints(testcase.Fingerprints);
            }

            var result = Subject.Identify(tracks, specifiedArtist, null, null, testcase.NewDownload, testcase.SingleRelease, false);

            TestLogger.Debug($"Found releases:\n{result.Where(x => x.AlbumRelease != null).Select(x => x.AlbumRelease?.ForeignReleaseId).ToJson()}");

            result.Should().HaveCount(testcase.ExpectedMusicBrainzReleaseIds.Count);
            result.Where(x => x.AlbumRelease != null).Select(x => x.AlbumRelease.ForeignReleaseId).Should().BeEquivalentTo(testcase.ExpectedMusicBrainzReleaseIds);
        }
    }
}
