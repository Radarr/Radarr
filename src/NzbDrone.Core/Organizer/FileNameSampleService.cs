using System.Collections.Generic;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Music;
using NzbDrone.Core.MediaFiles.MediaInfo;

namespace NzbDrone.Core.Organizer
{
    public interface IFilenameSampleService
    {
        SampleResult GetStandardTrackSample(NamingConfig nameSpec);

        string GetArtistFolderSample(NamingConfig nameSpec);
        string GetAlbumFolderSample(NamingConfig nameSpec);
    }

    public class FileNameSampleService : IFilenameSampleService
    {
        private readonly IBuildFileNames _buildFileNames;

        private static Artist _standardArtist;
        private static Album _standardAlbum;
        private static Track _track1;
        private static List<Track> _singleTrack;
        private static TrackFile _singleTrackFile;

        public FileNameSampleService(IBuildFileNames buildFileNames)
        {
            _buildFileNames = buildFileNames;

            _standardArtist = new Artist
            {
                Name = "The Artist Name"
            };

            _standardAlbum = new Album
            {
                Title = "The Album Title",
                ReleaseDate = System.DateTime.Today,
                Media = new List<Medium>
                {
                    new Medium
                    {
                        Name = "CD 1: First Years",
                        Format = "CD",
                        Number = 1
                    }
                }
            };

            _track1 = new Track
            {
                AbsoluteTrackNumber = 3,
                MediumNumber = 1,
                
                Title = "Track Title (1)",
                
            };

            _singleTrack = new List<Track> { _track1 };

            var mediaInfo = new MediaInfoModel()
            {
                VideoCodec = "AVC",
                VideoBitDepth = 8,
                AudioFormat = "FLAC",
                AudioChannels = 6,
                AudioChannelPositions = "3/2/0.1",
                AudioLanguages = "English",
                Subtitles = "English/German"
            };

            _singleTrackFile = new TrackFile
            {
                Quality = new QualityModel(Quality.MP3_256, new Revision(2)),
                RelativePath = "Artist.Name.Album.Name.TrackNum.Track.Title.MP3256.mp3",
                SceneName = "Artist.Name.Album.Name.TrackNum.Track.Title.MP3256",
                ReleaseGroup = "RlsGrp",
                MediaInfo = mediaInfo
            };

        }

        public SampleResult GetStandardTrackSample(NamingConfig nameSpec)
        {
            var result = new SampleResult
            {
                FileName = BuildTrackSample(_singleTrack, _standardArtist, _standardAlbum, _singleTrackFile, nameSpec),
                Artist = _standardArtist,
                Album = _standardAlbum,
                Tracks = _singleTrack,
                TrackFile = _singleTrackFile
            };

            return result;
        }

        public string GetArtistFolderSample(NamingConfig nameSpec)
        {
            return _buildFileNames.GetArtistFolder(_standardArtist, nameSpec);
        }

        public string GetAlbumFolderSample(NamingConfig nameSpec)
        {
            return _buildFileNames.GetAlbumFolder(_standardArtist, _standardAlbum, nameSpec);
        }

        private string BuildTrackSample(List<Track> tracks, Artist artist, Album album, TrackFile trackFile, NamingConfig nameSpec)
        {
            try
            {
                return _buildFileNames.BuildTrackFileName(tracks, artist, album, trackFile, nameSpec);
            }
            catch (NamingFormatException)
            {
                return string.Empty;
            }
        }
    }
}
