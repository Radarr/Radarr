using NzbDrone.Common.Extensions;
using NzbDrone.Core.Parser.Model;
using System.Linq;
using System.Collections.Generic;
using System;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Parser;
using NzbDrone.Common.Instrumentation;
using NLog;
using TagLib;
using TagLib.Id3v2;
using NLog.Fluent;
using NzbDrone.Common.Instrumentation.Extensions;
using System.Globalization;

namespace NzbDrone.Core.MediaFiles
{
    public class AudioTag
    {
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(AudioTag));

        public string Title { get; set; }
        public string[] Performers { get; set; }
        public string[] AlbumArtists { get; set; }
        public uint Track { get; set; }
        public uint TrackCount { get; set; }
        public string Album { get; set; }
        public uint Disc { get; set; }
        public uint DiscCount { get; set; }
        public string Media { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? OriginalReleaseDate { get; set; }
        public uint Year { get; set; }
        public uint OriginalYear { get; set; }
        public string Publisher { get; set; }
        public TimeSpan Duration { get; set; }
        public string[] Genres { get; set; }
        public string ImageFile { get; set; }
        public long ImageSize { get; set; }
        public string MusicBrainzReleaseCountry { get; set; }
        public string MusicBrainzReleaseStatus { get; set; }
        public string MusicBrainzReleaseType { get; set; }
        public string MusicBrainzReleaseId { get; set; }
        public string MusicBrainzArtistId { get; set; }
        public string MusicBrainzReleaseArtistId { get; set; }
        public string MusicBrainzReleaseGroupId { get; set; }
        public string MusicBrainzTrackId { get; set; }
        public string MusicBrainzReleaseTrackId { get; set; }
        public string MusicBrainzAlbumComment { get; set; }

        public bool IsValid { get; private set; }
        public QualityModel Quality { get; set; }
        public MediaInfoModel MediaInfo { get; set; }

        public AudioTag()
        {
            IsValid = true;
        }

        public AudioTag(string path)
        {
            Read(path);
        }

        public void Read(string path)
        {
            Logger.Debug($"Starting tag read for {path}");

            IsValid = false;
            TagLib.File file = null;
            try
            {
                file = TagLib.File.Create(path);
                var tag = file.Tag;

                Title = tag.Title ?? tag.TitleSort;
                Performers = tag.Performers ?? tag.PerformersSort;
                AlbumArtists = tag.AlbumArtists ?? tag.AlbumArtistsSort;
                Track = tag.Track;
                TrackCount = tag.TrackCount;
                Album = tag.Album ?? tag.AlbumSort;
                Disc = tag.Disc;
                DiscCount = tag.DiscCount;
                Year = tag.Year;
                Publisher = tag.Publisher;
                Duration = file.Properties.Duration;
                Genres = tag.Genres;
                ImageSize = tag.Pictures.FirstOrDefault()?.Data.Count ?? 0;
                MusicBrainzReleaseCountry = tag.MusicBrainzReleaseCountry;
                MusicBrainzReleaseStatus = tag.MusicBrainzReleaseStatus;
                MusicBrainzReleaseType = tag.MusicBrainzReleaseType;
                MusicBrainzReleaseId = tag.MusicBrainzReleaseId;
                MusicBrainzArtistId = tag.MusicBrainzArtistId;
                MusicBrainzReleaseArtistId = tag.MusicBrainzReleaseArtistId;
                MusicBrainzReleaseGroupId = tag.MusicBrainzReleaseGroupId;
                MusicBrainzTrackId = tag.MusicBrainzTrackId;

                DateTime tempDate;

                // Do the ones that aren't handled by the generic taglib implementation
                if (file.TagTypesOnDisk.HasFlag(TagTypes.Id3v2))
                {
                    var id3tag = (TagLib.Id3v2.Tag) file.GetTag(TagTypes.Id3v2);
                    Media = id3tag.GetTextAsString("TMED");
                    Date = ReadId3Date(id3tag, "TDRC");
                    OriginalReleaseDate = ReadId3Date(id3tag, "TDOR");
                    MusicBrainzAlbumComment = UserTextInformationFrame.Get(id3tag, "MusicBrainz Album Comment", false)?.Text.ExclusiveOrDefault();
                    MusicBrainzReleaseTrackId =  UserTextInformationFrame.Get(id3tag, "MusicBrainz Release Track Id", false)?.Text.ExclusiveOrDefault();
                }
                else if (file.TagTypesOnDisk.HasFlag(TagTypes.Xiph))
                {
                    // while publisher is handled by taglib, it seems to be mapped to 'ORGANIZATION' and not 'LABEL' like Picard is
                    // https://picard.musicbrainz.org/docs/mappings/
                    var flactag = (TagLib.Ogg.XiphComment) file.GetTag(TagLib.TagTypes.Xiph);
                    Media = flactag.GetField("MEDIA").ExclusiveOrDefault();
                    Date = DateTime.TryParse(flactag.GetField("DATE").ExclusiveOrDefault(), out tempDate) ? tempDate : default(DateTime?);
                    OriginalReleaseDate = DateTime.TryParse(flactag.GetField("ORIGINALDATE").ExclusiveOrDefault(), out tempDate) ? tempDate : default(DateTime?);
                    Publisher = flactag.GetField("LABEL").ExclusiveOrDefault();
                    MusicBrainzAlbumComment = flactag.GetField("MUSICBRAINZ_ALBUMCOMMENT").ExclusiveOrDefault();
                    MusicBrainzReleaseTrackId = flactag.GetField("MUSICBRAINZ_RELEASETRACKID").ExclusiveOrDefault();

                    // If we haven't managed to read status/type, try the alternate mapping
                    if (MusicBrainzReleaseStatus.IsNullOrWhiteSpace())
                    {
                        MusicBrainzReleaseStatus = flactag.GetField("RELEASESTATUS").ExclusiveOrDefault();
                    }

                    if (MusicBrainzReleaseType.IsNullOrWhiteSpace())
                    {
                        MusicBrainzReleaseType = flactag.GetField("RELEASETYPE").ExclusiveOrDefault();
                    }
                }
                else if (file.TagTypesOnDisk.HasFlag(TagTypes.Ape))
                {
                    var apetag = (TagLib.Ape.Tag) file.GetTag(TagTypes.Ape);
                    Media = apetag.GetItem("Media")?.ToString();
                    Date = DateTime.TryParse(apetag.GetItem("Year")?.ToString(), out tempDate) ? tempDate : default(DateTime?);
                    OriginalReleaseDate = DateTime.TryParse(apetag.GetItem("Original Date")?.ToString(), out tempDate) ? tempDate : default(DateTime?);
                    Publisher = apetag.GetItem("Label")?.ToString();
                    MusicBrainzAlbumComment = apetag.GetItem("MUSICBRAINZ_ALBUMCOMMENT")?.ToString();
                    MusicBrainzReleaseTrackId = apetag.GetItem("MUSICBRAINZ_RELEASETRACKID")?.ToString();
                }
                else if (file.TagTypesOnDisk.HasFlag(TagTypes.Asf))
                {
                    var asftag = (TagLib.Asf.Tag) file.GetTag(TagTypes.Asf);
                    Media = asftag.GetDescriptorString("WM/Media");
                    Date = DateTime.TryParse(asftag.GetDescriptorString("WM/Year"), out tempDate) ? tempDate : default(DateTime?);
                    OriginalReleaseDate = DateTime.TryParse(asftag.GetDescriptorString("WM/OriginalReleaseTime"), out tempDate) ? tempDate : default(DateTime?);
                    Publisher = asftag.GetDescriptorString("WM/Publisher");
                    MusicBrainzAlbumComment = asftag.GetDescriptorString("MusicBrainz/Album Comment");
                    MusicBrainzReleaseTrackId = asftag.GetDescriptorString("MusicBrainz/Release Track Id");
                }
                else if (file.TagTypesOnDisk.HasFlag(TagTypes.Apple))
                {
                    var appletag = (TagLib.Mpeg4.AppleTag) file.GetTag(TagTypes.Apple);
                    Media = appletag.GetDashBox("com.apple.iTunes", "MEDIA");
                    Date = DateTime.TryParse(appletag.DataBoxes(FixAppleId("day")).FirstOrDefault()?.Text, out tempDate) ? tempDate : default(DateTime?);
                    OriginalReleaseDate = DateTime.TryParse(appletag.GetDashBox("com.apple.iTunes", "Original Date"), out tempDate) ? tempDate : default(DateTime?);
                    MusicBrainzAlbumComment = appletag.GetDashBox("com.apple.iTunes", "MusicBrainz Album Comment");
                    MusicBrainzReleaseTrackId = appletag.GetDashBox("com.apple.iTunes", "MusicBrainz Release Track Id");
                }

                OriginalYear = OriginalReleaseDate.HasValue ? (uint)OriginalReleaseDate?.Year : 0;

                foreach (ICodec codec in file.Properties.Codecs)
                {
                    IAudioCodec acodec = codec as IAudioCodec;

                    if (acodec != null && (acodec.MediaTypes & MediaTypes.Audio) != MediaTypes.None)
                    {
                        int bitrate = acodec.AudioBitrate;
                        if (bitrate == 0)
                        {
                            // Taglib can't read bitrate for Opus.
                            bitrate = EstimateBitrate(file, path);
                        }

                        Logger.Debug("Audio Properties: " + acodec.Description + ", Bitrate: " + bitrate + ", Sample Size: " +
                                     file.Properties.BitsPerSample + ", SampleRate: " + acodec.AudioSampleRate + ", Channels: " + acodec.AudioChannels);

                        Quality = QualityParser.ParseQuality(file.Name, acodec.Description, bitrate, file.Properties.BitsPerSample);
                        Logger.Debug($"Quality parsed: {Quality}, Source: {Quality.QualityDetectionSource}");

                        MediaInfo = new MediaInfoModel {
                            AudioFormat = acodec.Description,
                            AudioBitrate = bitrate,
                            AudioChannels = acodec.AudioChannels,
                            AudioBits = file.Properties.BitsPerSample,
                            AudioSampleRate = acodec.AudioSampleRate
                        };
                    }
                }

                IsValid = true;
            }
            catch (Exception ex)
            {
                if (ex is CorruptFileException)
                {
                    Logger.Warn(ex, $"Tag reading failed for {path}.  File is corrupt");
                }
                else
                {
                    // Log as error so it goes to sentry with correct fingerprint
                    Logger.Error(ex, "Tag reading failed for {0}", path);
                }
            }
            finally
            {
                file?.Dispose();
            }

            // make sure these are initialized to avoid errors later on
            if (Quality == null)
            {
                Quality = QualityParser.ParseQuality(path, null, EstimateBitrate(file, path));
                Logger.Debug($"Unable to parse qulity from tag, Quality parsed from file path: {Quality}, Source: {Quality.QualityDetectionSource}");
            }

            MediaInfo = MediaInfo ?? new MediaInfoModel();
        }

        private int EstimateBitrate(TagLib.File file, string path)
        {
            int bitrate = 0;
            try
            {
                // Taglib File.Length is unreliable so use System.IO
                var size = new System.IO.FileInfo(path).Length;
                var duration = file.Properties.Duration.TotalSeconds;
                bitrate = (int) ((size * 8L) / (duration * 1024));

                Logger.Trace($"Estimating bitrate. Size: {size} Duration: {duration} Bitrate: {bitrate}");
            }
            catch
            {
            }

            return bitrate;
        }

        private DateTime? ReadId3Date(TagLib.Id3v2.Tag tag, string dateTag)
        {
            string date = tag.GetTextAsString(dateTag);

            if (tag.Version == 4)
            {
                // the unabused TDRC/TDOR tags
                return DateTime.TryParse(date, out DateTime result) ? result : default(DateTime?);
            }
            else if (dateTag == "TDRC")
            {
                // taglib maps the v3 TYER and TDAT to TDRC but does it incorrectly
                return DateTime.TryParseExact(date, "yyyy-dd-MM", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result) ? result : default(DateTime?);
            }
            else
            {
                // taglib maps the v3 TORY to TDRC so we just get a year
                return Int32.TryParse(date, out int year) && year >= 1860 && year <= DateTime.UtcNow.Year + 1 ? new DateTime(year, 1, 1) : default(DateTime?);
            }
        }

        private void WriteId3Date(TagLib.Id3v2.Tag tag, string v4field, string v3yyyy, string v3ddmm, DateTime? date)
        {
            if (tag.Version == 4)
            {
                tag.SetTextFrame(v3yyyy, default(string));
                if (v3ddmm.IsNotNullOrWhiteSpace())
                {
                    tag.SetTextFrame(v3ddmm, default(string));
                }
                tag.SetTextFrame(v4field, date.HasValue ? date.Value.ToString("yyyy-MM-dd") : null);
            }
            else
            {
                tag.SetTextFrame(v4field, default(string));
                tag.SetTextFrame(v3yyyy, date.HasValue ? date.Value.ToString("yyyy") : null);
                if (v3ddmm.IsNotNullOrWhiteSpace())
                {
                    tag.SetTextFrame(v3ddmm, date.HasValue ? date.Value.ToString("ddMM") : null);
                }
            }
        }

        private void WriteId3Tag(TagLib.Id3v2.Tag tag, string id, string value)
        {
            var frame = UserTextInformationFrame.Get(tag, id, true);

            if (value.IsNotNullOrWhiteSpace())
            {
                frame.Text = value.Split(';');
            }
            else
            {
                tag.RemoveFrame(frame);
            }
        }

		private static ReadOnlyByteVector FixAppleId(ByteVector id)
		{
			if (id.Count == 4) {
				var roid = id as ReadOnlyByteVector;
				if (roid != null)
					return roid;

				return new ReadOnlyByteVector(id);
			}

			if (id.Count == 3)
				return new ReadOnlyByteVector(0xa9, id[0], id[1], id[2]);

			return null;
		}

        public void Write(string path)
        {
            Logger.Debug($"Starting tag write for {path}");

            // patch up any null fields to work around TagLib exception for
            // WMA with null performers/albumartists
            Performers = Performers ?? new string[0];
            AlbumArtists = AlbumArtists ?? new string[0];
            Genres = Genres ?? new string[0];

            TagLib.File file = null;
            try
            {
                file = TagLib.File.Create(path);
                var tag = file.Tag;

                // do the ones with direct support in TagLib
                tag.Title = Title;
                tag.Performers = Performers;
                tag.AlbumArtists = AlbumArtists;
                tag.Track = Track;
                tag.TrackCount = TrackCount;
                tag.Album = Album;
                tag.Disc = Disc;
                tag.DiscCount = DiscCount;
                tag.Publisher = Publisher;
                tag.Genres = Genres;
                tag.MusicBrainzReleaseCountry = MusicBrainzReleaseCountry;
                tag.MusicBrainzReleaseStatus = MusicBrainzReleaseStatus;
                tag.MusicBrainzReleaseType = MusicBrainzReleaseType;
                tag.MusicBrainzReleaseId = MusicBrainzReleaseId;
                tag.MusicBrainzArtistId = MusicBrainzArtistId;
                tag.MusicBrainzReleaseArtistId = MusicBrainzReleaseArtistId;
                tag.MusicBrainzReleaseGroupId = MusicBrainzReleaseGroupId;
                tag.MusicBrainzTrackId = MusicBrainzTrackId;

                if (ImageFile.IsNotNullOrWhiteSpace())
                {
                    tag.Pictures = new IPicture[1] { new Picture(ImageFile) };
                }

                if (file.TagTypes.HasFlag(TagTypes.Id3v2))
                {
                    var id3tag = (TagLib.Id3v2.Tag) file.GetTag(TagTypes.Id3v2);
                    id3tag.SetTextFrame("TMED", Media);
                    WriteId3Date(id3tag, "TDRC", "TYER", "TDAT", Date);
                    WriteId3Date(id3tag, "TDOR", "TORY", null, OriginalReleaseDate);
                    WriteId3Tag(id3tag, "MusicBrainz Album Comment", MusicBrainzAlbumComment);
                    WriteId3Tag(id3tag, "MusicBrainz Release Track Id", MusicBrainzReleaseTrackId);
                }
                else if (file.TagTypes.HasFlag(TagTypes.Xiph))
                {
                    // while publisher is handled by taglib, it seems to be mapped to 'ORGANIZATION' and not 'LABEL' like Picard is
                    // https://picard.musicbrainz.org/docs/mappings/
                    tag.Publisher = null;
                    // taglib inserts leading zeros so set manually
                    tag.Track = 0;

                    var flactag = (TagLib.Ogg.XiphComment) file.GetTag(TagLib.TagTypes.Xiph);

                    flactag.SetField("DATE", Date.HasValue ? Date.Value.ToString("yyyy-MM-dd") : null);
                    flactag.SetField("ORIGINALDATE", OriginalReleaseDate.HasValue ? OriginalReleaseDate.Value.ToString("yyyy-MM-dd") : null);
                    flactag.SetField("ORIGINALYEAR", OriginalReleaseDate.HasValue ? OriginalReleaseDate.Value.Year.ToString() : null);
                    flactag.SetField("TRACKTOTAL", TrackCount);
                    flactag.SetField("TOTALTRACKS", TrackCount);
                    flactag.SetField("TRACKNUMBER", Track);
                    flactag.SetField("TOTALDISCS", DiscCount);
                    flactag.SetField("MEDIA", Media);
                    flactag.SetField("LABEL", Publisher);
                    flactag.SetField("MUSICBRAINZ_ALBUMCOMMENT", MusicBrainzAlbumComment);
                    flactag.SetField("MUSICBRAINZ_RELEASETRACKID", MusicBrainzReleaseTrackId);

                    // Add the alternate mappings used by picard (we write both)
                    flactag.SetField("RELEASESTATUS", MusicBrainzReleaseStatus);
                    flactag.SetField("RELEASETYPE", MusicBrainzReleaseType);
                }
                else if (file.TagTypes.HasFlag(TagTypes.Ape))
                {
                    var apetag = (TagLib.Ape.Tag) file.GetTag(TagTypes.Ape);

                    apetag.SetValue("Year", Date.HasValue ? Date.Value.ToString("yyyy-MM-dd") : null);
                    apetag.SetValue("Original Date", OriginalReleaseDate.HasValue ? OriginalReleaseDate.Value.ToString("yyyy-MM-dd") : null);
                    apetag.SetValue("Original Year", OriginalReleaseDate.HasValue ? OriginalReleaseDate.Value.Year.ToString() : null);
                    apetag.SetValue("Media", Media);
                    apetag.SetValue("Label", Publisher);
                    apetag.SetValue("MUSICBRAINZ_ALBUMCOMMENT", MusicBrainzAlbumComment);
                    apetag.SetValue("MUSICBRAINZ_RELEASETRACKID", MusicBrainzReleaseTrackId);
                }
                else if (file.TagTypes.HasFlag(TagTypes.Asf))
                {
                    var asftag = (TagLib.Asf.Tag) file.GetTag(TagTypes.Asf);

                    asftag.SetDescriptorString(Date.HasValue ? Date.Value.ToString("yyyy-MM-dd") : null, "WM/Year");
                    asftag.SetDescriptorString(OriginalReleaseDate.HasValue ? OriginalReleaseDate.Value.ToString("yyyy-MM-dd") : null, "WM/OriginalReleaseTime");
                    asftag.SetDescriptorString(OriginalReleaseDate.HasValue ? OriginalReleaseDate.Value.Year.ToString() : null, "WM/OriginalReleaseYear");
                    asftag.SetDescriptorString(Media, "WM/Media");
                    asftag.SetDescriptorString(Publisher, "WM/Publisher");
                    asftag.SetDescriptorString(MusicBrainzAlbumComment, "MusicBrainz/Album Comment");
                    asftag.SetDescriptorString(MusicBrainzReleaseTrackId, "MusicBrainz/Release Track Id");
                }
                else if (file.TagTypes.HasFlag(TagTypes.Apple))
                {
                    var appletag = (TagLib.Mpeg4.AppleTag) file.GetTag(TagTypes.Apple);

                    appletag.SetText(FixAppleId("day"), Date.HasValue ? Date.Value.ToString("yyyy-MM-dd") : null);
                    appletag.SetDashBox("com.apple.iTunes", "Original Date", OriginalReleaseDate.HasValue ? OriginalReleaseDate.Value.ToString("yyyy-MM-dd") : null);
                    appletag.SetDashBox("com.apple.iTunes", "Original Year", OriginalReleaseDate.HasValue ? OriginalReleaseDate.Value.Year.ToString() : null);
                    appletag.SetDashBox("com.apple.iTunes", "MEDIA", Media);
                    appletag.SetDashBox("com.apple.iTunes", "MusicBrainz Album Comment", MusicBrainzAlbumComment);
                    appletag.SetDashBox("com.apple.iTunes", "MusicBrainz Release Track Id", MusicBrainzReleaseTrackId);
                }

                file.Save();
            }
            catch (CorruptFileException ex)
            {
                Logger.Warn(ex, $"Tag writing failed for {path}.  File is corrupt");
            }
            catch (Exception ex)
            {
                Logger.Warn()
                    .Exception(ex)
                    .Message($"Tag writing failed for {path}")
                    .WriteSentryWarn("Tag writing failed")
                    .Write();
            }
            finally
            {
                file?.Dispose();
            }
        }

        public Dictionary<string, Tuple<string, string>> Diff(AudioTag other)
        {
            var output = new Dictionary<string, Tuple<string, string>>();

            if (!IsValid || !other.IsValid)
            {
                return output;
            }

            if (Title != other.Title)
            {
                output.Add("Title", Tuple.Create(Title, other.Title));
            }

            if (!Performers.SequenceEqual(other.Performers))
            {
                var oldValue = Performers.Any() ? string.Join(" / ", Performers) : null;
                var newValue = other.Performers.Any() ? string.Join(" / ", other.Performers) : null;

                output.Add("Artist", Tuple.Create(oldValue, newValue));
            }

            if (Album != other.Album)
            {
                output.Add("Album", Tuple.Create(Album, other.Album));
            }

            if (!AlbumArtists.SequenceEqual(other.AlbumArtists))
            {
                var oldValue = AlbumArtists.Any() ? string.Join(" / ", AlbumArtists) : null;
                var newValue = other.AlbumArtists.Any() ? string.Join(" / ", other.AlbumArtists) : null;

                output.Add("Album Artist", Tuple.Create(oldValue, newValue));
            }

            if (Track != other.Track)
            {
                output.Add("Track", Tuple.Create(Track.ToString(), other.Track.ToString()));
            }

            if (TrackCount != other.TrackCount)
            {
                output.Add("Track Count", Tuple.Create(TrackCount.ToString(), other.TrackCount.ToString()));
            }

            if (Disc != other.Disc)
            {
                output.Add("Disc", Tuple.Create(Disc.ToString(), other.Disc.ToString()));
            }

            if (DiscCount != other.DiscCount)
            {
                output.Add("Disc Count", Tuple.Create(DiscCount.ToString(), other.DiscCount.ToString()));
            }

            if (Media != other.Media)
            {
                output.Add("Media Format", Tuple.Create(Media, other.Media));
            }

            if (Date != other.Date)
            {
                var oldValue = Date.HasValue ? Date.Value.ToString("yyyy-MM-dd") : null;
                var newValue = other.Date.HasValue ? other.Date.Value.ToString("yyyy-MM-dd") : null;
                output.Add("Date", Tuple.Create(oldValue, newValue));
            }

            if (OriginalReleaseDate != other.OriginalReleaseDate)
            {
                // Id3v2.3 tags can only store the year, not the full date
                if (OriginalReleaseDate.HasValue &&
                    OriginalReleaseDate.Value.Month == 1 &&
                    OriginalReleaseDate.Value.Day == 1)
                {
                    if (OriginalReleaseDate.Value.Year != other.OriginalReleaseDate.Value.Year)
                    {
                        output.Add("Original Year", Tuple.Create(OriginalReleaseDate.Value.Year.ToString(), other.OriginalReleaseDate.Value.Year.ToString()));
                    }
                }
                else
                {
                    var oldValue = OriginalReleaseDate.HasValue ? OriginalReleaseDate.Value.ToString("yyyy-MM-dd") : null;
                    var newValue = other.OriginalReleaseDate.HasValue ? other.OriginalReleaseDate.Value.ToString("yyyy-MM-dd") : null;
                    output.Add("Original Release Date", Tuple.Create(oldValue, newValue));
                }
            }

            if (Publisher != other.Publisher)
            {
                output.Add("Label", Tuple.Create(Publisher, other.Publisher));
            }

            if (!Genres.SequenceEqual(other.Genres))
            {
                output.Add("Genres", Tuple.Create(string.Join(" / ", Genres), string.Join(" / ", other.Genres)));
            }

            if (ImageSize != other.ImageSize)
            {
                output.Add("Image Size", Tuple.Create(ImageSize.ToString(), other.ImageSize.ToString()));
            }

            return output;
        }

        public static implicit operator ParsedTrackInfo (AudioTag tag)
        {
            if (!tag.IsValid)
            {
                return new ParsedTrackInfo {
                    Quality = tag.Quality ?? new QualityModel { Quality = NzbDrone.Core.Qualities.Quality.Unknown },
                    MediaInfo = tag.MediaInfo ?? new MediaInfoModel()
                };
            }

            var artist = tag.AlbumArtists?.FirstOrDefault();

            if (artist.IsNullOrWhiteSpace())
            {
                artist = tag.Performers?.FirstOrDefault();
            }

            var artistTitleInfo = new ArtistTitleInfo
            {
                Title = artist,
                Year = (int)tag.Year
            };

            return new ParsedTrackInfo {
                AlbumTitle = tag.Album,
                ArtistTitle = artist,
                ArtistMBId = tag.MusicBrainzReleaseArtistId,
                AlbumMBId = tag.MusicBrainzReleaseGroupId,
                ReleaseMBId = tag.MusicBrainzReleaseId,
                // SIC: the recording ID is stored in this field.
                // See https://picard.musicbrainz.org/docs/mappings/
                RecordingMBId = tag.MusicBrainzTrackId,
                TrackMBId = tag.MusicBrainzReleaseTrackId,
                DiscNumber = (int)tag.Disc,
                DiscCount = (int)tag.DiscCount,
                Year = tag.Year,
                Label = tag.Publisher,
                TrackNumbers = new [] { (int) tag.Track },
                ArtistTitleInfo = artistTitleInfo,
                Title = tag.Title,
                CleanTitle = tag.Title?.CleanTrackTitle(),
                Country = IsoCountries.Find(tag.MusicBrainzReleaseCountry),
                Duration = tag.Duration,
                Disambiguation = tag.MusicBrainzAlbumComment,
                Quality = tag.Quality,
                MediaInfo = tag.MediaInfo
            };
        }
    }
}
