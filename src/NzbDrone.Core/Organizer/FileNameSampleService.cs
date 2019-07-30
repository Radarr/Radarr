using System.Collections.Generic;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Music;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Organizer
{
    public interface IFilenameSampleService
    {
        SampleResult GetStandardTrackSample(NamingConfig nameSpec);
        SampleResult GetMultiDiscTrackSample(NamingConfig nameSpec);
        string GetArtistFolderSample(NamingConfig nameSpec);
        string GetAlbumFolderSample(NamingConfig nameSpec);
    }

    public class FileNameSampleService : IFilenameSampleService
    {
        private readonly IBuildFileNames _buildFileNames;

        private static Artist _standardArtist;
        private static Album _standardAlbum;
        private static AlbumRelease _singleRelease;
        private static AlbumRelease _multiRelease;
        private static Track _track1;
        private static List<Track> _singleTrack;
        private static TrackFile _singleTrackFile;
        private static List<string> _preferredWords;

        public FileNameSampleService(IBuildFileNames buildFileNames)
        {
            _buildFileNames = buildFileNames;

            _standardArtist = new Artist
            {
                Metadata = new ArtistMetadata
                {
                    Name = "The Artist Name",
                    Disambiguation = "US Rock Band"
                    
                }
            };

            _standardAlbum = new Album
            {
                Title = "The Album Title",
                ReleaseDate = System.DateTime.Today,
                AlbumType = "Album",
                Disambiguation = "The Best Album",
            };

            _singleRelease = new AlbumRelease
            {
                Album = _standardAlbum,
                Media = new List<Medium>
                {
                    new Medium
                    {
                        Name = "CD 1: First Years",
                        Format = "CD",
                        Number = 1
                    }
                },
                Monitored = true
            };

            _multiRelease = new AlbumRelease
            {
                Album = _standardAlbum,
                Media = new List<Medium>
                {
                    new Medium
                    {
                        Name = "CD 1: First Years",
                        Format = "CD",
                        Number = 1
                    },
                    new Medium
                    {
                        Name = "CD 2: Second Best",
                        Format = "CD",
                        Number = 2
                    }
                },
                Monitored = true
            };

            _track1 = new Track
            {
                AlbumRelease = _singleRelease,
                AbsoluteTrackNumber = 3,
                MediumNumber = 1,
                
                Title = "Track Title (1)",
                
            };

            _singleTrack = new List<Track> { _track1 };

            var mediaInfo = new MediaInfoModel()
            {
                AudioFormat = "Flac Audio",
                AudioChannels = 2,
                AudioBitrate = 875,
                AudioBits = 24,
                AudioSampleRate = 44100
            };

            _singleTrackFile = new TrackFile
            {
                Quality = new QualityModel(Quality.MP3_256, new Revision(2)),
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
            _track1.AlbumRelease = _singleRelease;

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

        public SampleResult GetMultiDiscTrackSample(NamingConfig nameSpec)
        {
            _track1.AlbumRelease = _multiRelease;

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
                return _buildFileNames.BuildTrackFileName(tracks, artist, album, trackFile, nameSpec, _preferredWords);
            }
            catch (NamingFormatException)
            {
                return string.Empty;
            }
        }
    }
}
