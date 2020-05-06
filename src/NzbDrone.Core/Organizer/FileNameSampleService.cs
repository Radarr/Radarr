using System.Collections.Generic;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Music;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Organizer
{
    public interface IFilenameSampleService
    {
        SampleResult GetStandardTrackSample(NamingConfig nameSpec);
        SampleResult GetMultiDiscTrackSample(NamingConfig nameSpec);
        string GetArtistFolderSample(NamingConfig nameSpec);
    }

    public class FileNameSampleService : IFilenameSampleService
    {
        private readonly IBuildFileNames _buildFileNames;

        private static Author _standardArtist;
        private static Book _standardAlbum;
        private static BookFile _singleTrackFile;
        private static List<string> _preferredWords;

        public FileNameSampleService(IBuildFileNames buildFileNames)
        {
            _buildFileNames = buildFileNames;

            _standardArtist = new Author
            {
                Metadata = new AuthorMetadata
                {
                    Name = "The Author Name",
                    Disambiguation = "US Author"
                }
            };

            _standardAlbum = new Book
            {
                Title = "The Book Title",
                ReleaseDate = System.DateTime.Today,
                Disambiguation = "First Book"
            };

            var mediaInfo = new MediaInfoModel()
            {
                AudioFormat = "Flac Audio",
                AudioChannels = 2,
                AudioBitrate = 875,
                AudioBits = 24,
                AudioSampleRate = 44100
            };

            _singleTrackFile = new BookFile
            {
                Quality = new QualityModel(Quality.MP3_320, new Revision(2)),
                Path = "/music/Artist.Name.Album.Name.TrackNum.Track.Title.MP3256.mp3",
                SceneName = "Artist.Name.Album.Name.TrackNum.Track.Title.MP3256",
                ReleaseGroup = "RlsGrp",
                MediaInfo = mediaInfo
            };

            _preferredWords = new List<string>
            {
                "iNTERNAL"
            };
        }

        public SampleResult GetStandardTrackSample(NamingConfig nameSpec)
        {
            var result = new SampleResult
            {
                FileName = BuildTrackSample(_standardArtist, _standardAlbum, _singleTrackFile, nameSpec),
                Artist = _standardArtist,
                Album = _standardAlbum,
                TrackFile = _singleTrackFile
            };

            return result;
        }

        public SampleResult GetMultiDiscTrackSample(NamingConfig nameSpec)
        {
            var result = new SampleResult
            {
                FileName = BuildTrackSample(_standardArtist, _standardAlbum, _singleTrackFile, nameSpec),
                Artist = _standardArtist,
                Album = _standardAlbum,
                TrackFile = _singleTrackFile
            };

            return result;
        }

        public string GetArtistFolderSample(NamingConfig nameSpec)
        {
            return _buildFileNames.GetArtistFolder(_standardArtist, nameSpec);
        }

        private string BuildTrackSample(Author artist, Book album, BookFile trackFile, NamingConfig nameSpec)
        {
            try
            {
                return _buildFileNames.BuildTrackFileName(artist, album, trackFile, nameSpec, _preferredWords);
            }
            catch (NamingFormatException)
            {
                return string.Empty;
            }
        }
    }
}
