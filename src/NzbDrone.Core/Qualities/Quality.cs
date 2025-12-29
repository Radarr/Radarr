using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Qualities
{
    public class Quality : IEmbeddedDocument, IEquatable<Quality>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public QualitySource Source { get; set; }
        public int Resolution { get; set; }
        public Modifier Modifier { get; set; }

        public Quality()
        {
        }

        private Quality(int id, string name, QualitySource source, int resolution, Modifier modifier = Modifier.NONE)
        {
            Id = id;
            Name = name;
            Source = source;
            Resolution = resolution;
            Modifier = modifier;
        }

        public override string ToString()
        {
            return Name;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public bool Equals(Quality other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return Equals(obj as Quality);
        }

        public static bool operator ==(Quality left, Quality right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Quality left, Quality right)
        {
            return !Equals(left, right);
        }

        // Unable to determine
        public static Quality Unknown => new Quality(0, "Unknown", QualitySource.UNKNOWN, 0);

        // Pre-release
        public static Quality WORKPRINT => new Quality(24, "WORKPRINT", QualitySource.WORKPRINT, 0); // new
        public static Quality CAM => new Quality(25, "CAM", QualitySource.CAM, 0); // new
        public static Quality TELESYNC => new Quality(26, "TELESYNC", QualitySource.TELESYNC, 0); // new
        public static Quality TELECINE => new Quality(27, "TELECINE", QualitySource.TELECINE, 0); // new
        public static Quality DVDSCR => new Quality(28, "DVDSCR", QualitySource.DVD, 480, Modifier.SCREENER); // new
        public static Quality REGIONAL => new Quality(29, "REGIONAL", QualitySource.DVD, 480, Modifier.REGIONAL); // new

        // SD
        public static Quality SDTV => new Quality(1, "SDTV", QualitySource.TV, 480);
        public static Quality DVD => new Quality(2, "DVD", QualitySource.DVD, 0);
        public static Quality DVDR => new Quality(23, "DVD-R", QualitySource.DVD, 480, Modifier.REMUX); // new

        // HDTV
        public static Quality HDTV720p => new Quality(4, "HDTV-720p", QualitySource.TV, 720);
        public static Quality HDTV1080p => new Quality(9, "HDTV-1080p", QualitySource.TV, 1080);
        public static Quality HDTV2160p => new Quality(16, "HDTV-2160p", QualitySource.TV, 2160);

        // Web-DL
        public static Quality WEBDL480p => new Quality(8, "WEBDL-480p", QualitySource.WEBDL, 480);
        public static Quality WEBDL720p => new Quality(5, "WEBDL-720p", QualitySource.WEBDL, 720);
        public static Quality WEBDL1080p => new Quality(3, "WEBDL-1080p", QualitySource.WEBDL, 1080);
        public static Quality WEBDL2160p => new Quality(18, "WEBDL-2160p", QualitySource.WEBDL, 2160);

        // Bluray
        public static Quality Bluray480p => new Quality(20, "Bluray-480p", QualitySource.BLURAY, 480); // new
        public static Quality Bluray576p => new Quality(21, "Bluray-576p", QualitySource.BLURAY, 576); // new
        public static Quality Bluray720p => new Quality(6, "Bluray-720p", QualitySource.BLURAY, 720);
        public static Quality Bluray1080p => new Quality(7, "Bluray-1080p", QualitySource.BLURAY, 1080);
        public static Quality Bluray2160p => new Quality(19, "Bluray-2160p", QualitySource.BLURAY, 2160);

        public static Quality Remux1080p => new Quality(30, "Remux-1080p", QualitySource.BLURAY, 1080, Modifier.REMUX);
        public static Quality Remux2160p => new Quality(31, "Remux-2160p", QualitySource.BLURAY, 2160, Modifier.REMUX);

        public static Quality BRDISK => new Quality(22, "BR-DISK", QualitySource.BLURAY, 1080, Modifier.BRDISK); // new

        // Others
        public static Quality RAWHD => new Quality(10, "Raw-HD", QualitySource.TV, 1080, Modifier.RAWHD);

        public static Quality WEBRip480p => new Quality(12, "WEBRip-480p", QualitySource.WEBRIP, 480);
        public static Quality WEBRip720p => new Quality(14, "WEBRip-720p", QualitySource.WEBRIP, 720);
        public static Quality WEBRip1080p => new Quality(15, "WEBRip-1080p", QualitySource.WEBRIP, 1080);
        public static Quality WEBRip2160p => new Quality(17, "WEBRip-2160p", QualitySource.WEBRIP, 2160);

        // Book Qualities (IDs 100-109)
        public static Quality EbookUnknown => new Quality(100, "Unknown eBook", QualitySource.EBOOK, 0);
        public static Quality EPUB => new Quality(101, "EPUB", QualitySource.EBOOK, 0);
        public static Quality MOBI => new Quality(102, "MOBI", QualitySource.EBOOK, 0);
        public static Quality AZW3 => new Quality(103, "AZW3", QualitySource.EBOOK, 0);
        public static Quality PDF => new Quality(104, "PDF", QualitySource.EBOOK, 0);
        public static Quality TXT => new Quality(105, "TXT", QualitySource.EBOOK, 0);

        // Audiobook Qualities (IDs 200-209)
        public static Quality AudiobookUnknown => new Quality(200, "Unknown Audiobook", QualitySource.AUDIOBOOK, 0);
        public static Quality MP3_128 => new Quality(201, "MP3-128", QualitySource.AUDIOBOOK, 128);
        public static Quality MP3_320 => new Quality(202, "MP3-320", QualitySource.AUDIOBOOK, 320);
        public static Quality M4B => new Quality(203, "M4B", QualitySource.AUDIOBOOK, 256);
        public static Quality AudioFLAC => new Quality(204, "FLAC", QualitySource.AUDIOBOOK, 1411);

        // Music Qualities - Lossy (IDs 300-319)
        public static Quality MusicUnknown => new Quality(300, "Unknown Music", QualitySource.MUSIC, 0);
        public static Quality MusicMP3_128 => new Quality(301, "MP3-128", QualitySource.MUSIC, 128);
        public static Quality MusicMP3_192 => new Quality(302, "MP3-192", QualitySource.MUSIC, 192);
        public static Quality MusicMP3_256 => new Quality(303, "MP3-256", QualitySource.MUSIC, 256);
        public static Quality MusicMP3_320 => new Quality(304, "MP3-320", QualitySource.MUSIC, 320);
        public static Quality MusicAAC_128 => new Quality(305, "AAC-128", QualitySource.MUSIC, 128);
        public static Quality MusicAAC_256 => new Quality(306, "AAC-256", QualitySource.MUSIC, 256);
        public static Quality MusicAAC_320 => new Quality(307, "AAC-320", QualitySource.MUSIC, 320);
        public static Quality MusicOGG_128 => new Quality(308, "OGG-128", QualitySource.MUSIC, 128);
        public static Quality MusicOGG_192 => new Quality(309, "OGG-192", QualitySource.MUSIC, 192);
        public static Quality MusicOGG_256 => new Quality(310, "OGG-256", QualitySource.MUSIC, 256);
        public static Quality MusicOGG_320 => new Quality(311, "OGG-320", QualitySource.MUSIC, 320);
        public static Quality MusicOpus_128 => new Quality(312, "Opus-128", QualitySource.MUSIC, 128);
        public static Quality MusicOpus_192 => new Quality(313, "Opus-192", QualitySource.MUSIC, 192);
        public static Quality MusicOpus_256 => new Quality(314, "Opus-256", QualitySource.MUSIC, 256);
        public static Quality MusicWMA => new Quality(315, "WMA", QualitySource.MUSIC, 192);

        // Music Qualities - FLAC Lossless (IDs 320-329)
        public static Quality MusicFLAC_16_44 => new Quality(320, "FLAC 16/44.1", QualitySource.MUSIC, 1411);
        public static Quality MusicFLAC_16_48 => new Quality(321, "FLAC 16/48", QualitySource.MUSIC, 1536);
        public static Quality MusicFLAC_24_44 => new Quality(322, "FLAC 24/44.1", QualitySource.MUSIC, 2116);
        public static Quality MusicFLAC_24_48 => new Quality(323, "FLAC 24/48", QualitySource.MUSIC, 2304);
        public static Quality MusicFLAC_24_88 => new Quality(324, "FLAC 24/88.2", QualitySource.MUSIC, 4233);
        public static Quality MusicFLAC_24_96 => new Quality(325, "FLAC 24/96", QualitySource.MUSIC, 4608);
        public static Quality MusicFLAC_24_176 => new Quality(326, "FLAC 24/176.4", QualitySource.MUSIC, 8467);
        public static Quality MusicFLAC_24_192 => new Quality(327, "FLAC 24/192", QualitySource.MUSIC, 9216);

        // Music Qualities - WAV Lossless (IDs 340-349)
        public static Quality MusicWAV_16_44 => new Quality(340, "WAV 16/44.1", QualitySource.MUSIC, 1411);
        public static Quality MusicWAV_16_48 => new Quality(341, "WAV 16/48", QualitySource.MUSIC, 1536);
        public static Quality MusicWAV_24_44 => new Quality(342, "WAV 24/44.1", QualitySource.MUSIC, 2116);
        public static Quality MusicWAV_24_48 => new Quality(343, "WAV 24/48", QualitySource.MUSIC, 2304);
        public static Quality MusicWAV_24_88 => new Quality(344, "WAV 24/88.2", QualitySource.MUSIC, 4233);
        public static Quality MusicWAV_24_96 => new Quality(345, "WAV 24/96", QualitySource.MUSIC, 4608);
        public static Quality MusicWAV_24_176 => new Quality(346, "WAV 24/176.4", QualitySource.MUSIC, 8467);
        public static Quality MusicWAV_24_192 => new Quality(347, "WAV 24/192", QualitySource.MUSIC, 9216);

        // Music Qualities - AIFF Lossless (IDs 350-359)
        public static Quality MusicAIFF_16_44 => new Quality(350, "AIFF 16/44.1", QualitySource.MUSIC, 1411);
        public static Quality MusicAIFF_16_48 => new Quality(351, "AIFF 16/48", QualitySource.MUSIC, 1536);
        public static Quality MusicAIFF_24_44 => new Quality(352, "AIFF 24/44.1", QualitySource.MUSIC, 2116);
        public static Quality MusicAIFF_24_48 => new Quality(353, "AIFF 24/48", QualitySource.MUSIC, 2304);
        public static Quality MusicAIFF_24_88 => new Quality(354, "AIFF 24/88.2", QualitySource.MUSIC, 4233);
        public static Quality MusicAIFF_24_96 => new Quality(355, "AIFF 24/96", QualitySource.MUSIC, 4608);
        public static Quality MusicAIFF_24_176 => new Quality(356, "AIFF 24/176.4", QualitySource.MUSIC, 8467);
        public static Quality MusicAIFF_24_192 => new Quality(357, "AIFF 24/192", QualitySource.MUSIC, 9216);

        // Music Qualities - DSD (IDs 360-369)
        public static Quality MusicDSD64 => new Quality(360, "DSD64", QualitySource.MUSIC, 2822);
        public static Quality MusicDSD128 => new Quality(361, "DSD128", QualitySource.MUSIC, 5644);
        public static Quality MusicDSD256 => new Quality(362, "DSD256", QualitySource.MUSIC, 11289);
        public static Quality MusicDSD512 => new Quality(363, "DSD512", QualitySource.MUSIC, 22579);

        // Music Qualities - Other Lossless (IDs 370-379)
        public static Quality MusicALAC_16_44 => new Quality(370, "ALAC 16/44.1", QualitySource.MUSIC, 1411);
        public static Quality MusicALAC_16_48 => new Quality(371, "ALAC 16/48", QualitySource.MUSIC, 1536);
        public static Quality MusicALAC_24_44 => new Quality(372, "ALAC 24/44.1", QualitySource.MUSIC, 2116);
        public static Quality MusicALAC_24_48 => new Quality(373, "ALAC 24/48", QualitySource.MUSIC, 2304);
        public static Quality MusicALAC_24_96 => new Quality(374, "ALAC 24/96", QualitySource.MUSIC, 4608);
        public static Quality MusicALAC_24_192 => new Quality(375, "ALAC 24/192", QualitySource.MUSIC, 9216);
        public static Quality MusicAPE => new Quality(376, "APE", QualitySource.MUSIC, 1411);
        public static Quality MusicWavPack => new Quality(377, "WavPack", QualitySource.MUSIC, 1411);

        // Music Qualities - Special Formats (IDs 380-389)
        public static Quality MusicMQA => new Quality(380, "MQA", QualitySource.MUSIC, 1536);
        public static Quality MusicMQA_Studio => new Quality(381, "MQA Studio", QualitySource.MUSIC, 9216);

        // Quality Group Names (used in QualityDefinition)
        private const string GroupWeb480P = "WEB 480p";
        private const string GroupWeb720P = "WEB 720p";
        private const string GroupWeb1080P = "WEB 1080p";
        private const string GroupWeb2160P = "WEB 2160p";
        private const string GroupFlacCd = "FLAC CD Quality";
        private const string GroupFlacHiRes = "FLAC Hi-Res";
        private const string GroupFlacUltraHiRes = "FLAC Ultra Hi-Res";
        private const string GroupWavCd = "WAV CD Quality";
        private const string GroupWavHiRes = "WAV Hi-Res";
        private const string GroupWavUltraHiRes = "WAV Ultra Hi-Res";
        private const string GroupAiffCd = "AIFF CD Quality";
        private const string GroupAiffHiRes = "AIFF Hi-Res";
        private const string GroupAiffUltraHiRes = "AIFF Ultra Hi-Res";
        private const string GroupDsd = "DSD";
        private const string GroupAlacCd = "ALAC CD Quality";
        private const string GroupAlacHiRes = "ALAC Hi-Res";
        private const string GroupAlacUltraHiRes = "ALAC Ultra Hi-Res";
        private const string GroupMqa = "MQA";

        static Quality()
        {
            All = new List<Quality>
            {
                Unknown,
                WORKPRINT,
                CAM,
                TELESYNC,
                TELECINE,
                DVDSCR,
                REGIONAL,
                SDTV,
                DVD,
                DVDR,
                HDTV720p,
                HDTV1080p,
                HDTV2160p,
                WEBDL480p,
                WEBDL720p,
                WEBDL1080p,
                WEBDL2160p,
                WEBRip480p,
                WEBRip720p,
                WEBRip1080p,
                WEBRip2160p,
                Bluray480p,
                Bluray576p,
                Bluray720p,
                Bluray1080p,
                Bluray2160p,
                Remux1080p,
                Remux2160p,
                BRDISK,
                RAWHD,

                // Book qualities
                EbookUnknown,
                EPUB,
                MOBI,
                AZW3,
                PDF,
                TXT,

                // Audiobook qualities
                AudiobookUnknown,
                MP3_128,
                MP3_320,
                M4B,
                AudioFLAC,

                // Music qualities - Lossy
                MusicUnknown,
                MusicMP3_128,
                MusicMP3_192,
                MusicMP3_256,
                MusicMP3_320,
                MusicAAC_128,
                MusicAAC_256,
                MusicAAC_320,
                MusicOGG_128,
                MusicOGG_192,
                MusicOGG_256,
                MusicOGG_320,
                MusicOpus_128,
                MusicOpus_192,
                MusicOpus_256,
                MusicWMA,

                // Music qualities - FLAC
                MusicFLAC_16_44,
                MusicFLAC_16_48,
                MusicFLAC_24_44,
                MusicFLAC_24_48,
                MusicFLAC_24_88,
                MusicFLAC_24_96,
                MusicFLAC_24_176,
                MusicFLAC_24_192,

                // Music qualities - WAV
                MusicWAV_16_44,
                MusicWAV_16_48,
                MusicWAV_24_44,
                MusicWAV_24_48,
                MusicWAV_24_88,
                MusicWAV_24_96,
                MusicWAV_24_176,
                MusicWAV_24_192,

                // Music qualities - AIFF
                MusicAIFF_16_44,
                MusicAIFF_16_48,
                MusicAIFF_24_44,
                MusicAIFF_24_48,
                MusicAIFF_24_88,
                MusicAIFF_24_96,
                MusicAIFF_24_176,
                MusicAIFF_24_192,

                // Music qualities - DSD
                MusicDSD64,
                MusicDSD128,
                MusicDSD256,
                MusicDSD512,

                // Music qualities - Other Lossless
                MusicALAC_16_44,
                MusicALAC_16_48,
                MusicALAC_24_44,
                MusicALAC_24_48,
                MusicALAC_24_96,
                MusicALAC_24_192,
                MusicAPE,
                MusicWavPack,

                // Music qualities - Special
                MusicMQA,
                MusicMQA_Studio
            };

            AllLookup = new Quality[All.Select(v => v.Id).Max() + 1];
            foreach (var quality in All)
            {
                AllLookup[quality.Id] = quality;
            }

            DefaultQualityDefinitions = new HashSet<QualityDefinition>
            {
                new QualityDefinition(Quality.Unknown)     { Weight = 1,  MinSize = 0, MaxSize = 100, PreferredSize = 95 },
                new QualityDefinition(Quality.WORKPRINT)   { Weight = 2,  MinSize = 0, MaxSize = 100, PreferredSize = 95 },
                new QualityDefinition(Quality.CAM)         { Weight = 3,  MinSize = 0, MaxSize = 100, PreferredSize = 95 },
                new QualityDefinition(Quality.TELESYNC)    { Weight = 4,  MinSize = 0, MaxSize = 100, PreferredSize = 95 },
                new QualityDefinition(Quality.TELECINE)    { Weight = 5,  MinSize = 0, MaxSize = 100, PreferredSize = 95 },
                new QualityDefinition(Quality.REGIONAL)    { Weight = 6,  MinSize = 0, MaxSize = 100, PreferredSize = 95 },
                new QualityDefinition(Quality.DVDSCR)      { Weight = 7,  MinSize = 0, MaxSize = 100, PreferredSize = 95 },
                new QualityDefinition(Quality.SDTV)        { Weight = 8,  MinSize = 0, MaxSize = 100, PreferredSize = 95 },
                new QualityDefinition(Quality.DVD)         { Weight = 9,  MinSize = 0, MaxSize = 100, PreferredSize = 95 },
                new QualityDefinition(Quality.DVDR)        { Weight = 10,  MinSize = 0, MaxSize = 100, PreferredSize = 95 },

                new QualityDefinition(Quality.WEBDL480p)   { Weight = 11, MinSize = 0, MaxSize = 100, PreferredSize = 95, GroupName = GroupWeb480P },
                new QualityDefinition(Quality.WEBRip480p)   { Weight = 11, MinSize = 0, MaxSize = 100, PreferredSize = 95, GroupName = GroupWeb480P },
                new QualityDefinition(Quality.Bluray480p)  { Weight = 12, MinSize = 0, MaxSize = 100, PreferredSize = 95 },
                new QualityDefinition(Quality.Bluray576p)  { Weight = 13, MinSize = 0, MaxSize = 100, PreferredSize = 95 },

                new QualityDefinition(Quality.HDTV720p)    { Weight = 14, MinSize = 0, MaxSize = 100, PreferredSize = 95 },
                new QualityDefinition(Quality.WEBDL720p)   { Weight = 15, MinSize = 0, MaxSize = 100, PreferredSize = 95, GroupName = GroupWeb720P },
                new QualityDefinition(Quality.WEBRip720p)   { Weight = 15, MinSize = 0, MaxSize = 100, PreferredSize = 95, GroupName = GroupWeb720P },
                new QualityDefinition(Quality.Bluray720p)  { Weight = 16, MinSize = 0, MaxSize = 100, PreferredSize = 95 },

                new QualityDefinition(Quality.HDTV1080p)   { Weight = 17, MinSize = 0, MaxSize = 100, PreferredSize = 95 },
                new QualityDefinition(Quality.WEBDL1080p)  { Weight = 18, MinSize = 0, MaxSize = 100, PreferredSize = 95, GroupName = GroupWeb1080P },
                new QualityDefinition(Quality.WEBRip1080p)   { Weight = 18, MinSize = 0, MaxSize = 100, PreferredSize = 95, GroupName = GroupWeb1080P },
                new QualityDefinition(Quality.Bluray1080p) { Weight = 19, MinSize = 0, MaxSize = null, PreferredSize = null },
                new QualityDefinition(Quality.Remux1080p)  { Weight = 20, MinSize = 0, MaxSize = null, PreferredSize = null },

                new QualityDefinition(Quality.HDTV2160p)   { Weight = 21, MinSize = 0, MaxSize = null, PreferredSize = null },
                new QualityDefinition(Quality.WEBDL2160p)  { Weight = 22, MinSize = 0, MaxSize = null, PreferredSize = null, GroupName = GroupWeb2160P },
                new QualityDefinition(Quality.WEBRip2160p)  { Weight = 22, MinSize = 0, MaxSize = null, PreferredSize = null, GroupName = GroupWeb2160P },
                new QualityDefinition(Quality.Bluray2160p) { Weight = 23, MinSize = 0, MaxSize = null, PreferredSize = null },
                new QualityDefinition(Quality.Remux2160p)  { Weight = 24, MinSize = 0, MaxSize = null, PreferredSize = null },

                new QualityDefinition(Quality.BRDISK)      { Weight = 25, MinSize = 0, MaxSize = null, PreferredSize = null },
                new QualityDefinition(Quality.RAWHD)       { Weight = 26, MinSize = 0, MaxSize = null, PreferredSize = null },

                // Book quality definitions (Weight 100+)
                new QualityDefinition(Quality.EbookUnknown) { Weight = 100, MinSize = 0, MaxSize = 50, PreferredSize = 10 },
                new QualityDefinition(Quality.TXT)          { Weight = 101, MinSize = 0, MaxSize = 5, PreferredSize = 1 },
                new QualityDefinition(Quality.PDF)          { Weight = 102, MinSize = 0, MaxSize = 100, PreferredSize = 20 },
                new QualityDefinition(Quality.MOBI)         { Weight = 103, MinSize = 0, MaxSize = 50, PreferredSize = 5 },
                new QualityDefinition(Quality.AZW3)         { Weight = 104, MinSize = 0, MaxSize = 50, PreferredSize = 5 },
                new QualityDefinition(Quality.EPUB)         { Weight = 105, MinSize = 0, MaxSize = 50, PreferredSize = 5 },

                // Audiobook quality definitions (Weight 200+)
                new QualityDefinition(Quality.AudiobookUnknown) { Weight = 200, MinSize = 0, MaxSize = null, PreferredSize = null },
                new QualityDefinition(Quality.MP3_128)          { Weight = 201, MinSize = 0, MaxSize = 500, PreferredSize = 200 },
                new QualityDefinition(Quality.MP3_320)          { Weight = 202, MinSize = 0, MaxSize = 1000, PreferredSize = 500 },
                new QualityDefinition(Quality.M4B)              { Weight = 203, MinSize = 0, MaxSize = 1000, PreferredSize = 400 },
                new QualityDefinition(Quality.AudioFLAC)        { Weight = 204, MinSize = 0, MaxSize = null, PreferredSize = null },

                // Music quality definitions - Lossy (Weight 300-319)
                new QualityDefinition(Quality.MusicUnknown)   { Weight = 300, MinSize = 0, MaxSize = null, PreferredSize = null },
                new QualityDefinition(Quality.MusicMP3_128)   { Weight = 301, MinSize = 0, MaxSize = 10, PreferredSize = 5 },
                new QualityDefinition(Quality.MusicMP3_192)   { Weight = 302, MinSize = 0, MaxSize = 12, PreferredSize = 8 },
                new QualityDefinition(Quality.MusicMP3_256)   { Weight = 303, MinSize = 0, MaxSize = 15, PreferredSize = 10 },
                new QualityDefinition(Quality.MusicMP3_320)   { Weight = 304, MinSize = 0, MaxSize = 20, PreferredSize = 15 },
                new QualityDefinition(Quality.MusicAAC_128)   { Weight = 305, MinSize = 0, MaxSize = 10, PreferredSize = 5 },
                new QualityDefinition(Quality.MusicAAC_256)   { Weight = 306, MinSize = 0, MaxSize = 15, PreferredSize = 10 },
                new QualityDefinition(Quality.MusicAAC_320)   { Weight = 307, MinSize = 0, MaxSize = 20, PreferredSize = 15 },
                new QualityDefinition(Quality.MusicOGG_128)   { Weight = 308, MinSize = 0, MaxSize = 10, PreferredSize = 5 },
                new QualityDefinition(Quality.MusicOGG_192)   { Weight = 309, MinSize = 0, MaxSize = 12, PreferredSize = 8 },
                new QualityDefinition(Quality.MusicOGG_256)   { Weight = 310, MinSize = 0, MaxSize = 15, PreferredSize = 10 },
                new QualityDefinition(Quality.MusicOGG_320)   { Weight = 311, MinSize = 0, MaxSize = 20, PreferredSize = 15 },
                new QualityDefinition(Quality.MusicOpus_128)  { Weight = 312, MinSize = 0, MaxSize = 10, PreferredSize = 5 },
                new QualityDefinition(Quality.MusicOpus_192)  { Weight = 313, MinSize = 0, MaxSize = 12, PreferredSize = 8 },
                new QualityDefinition(Quality.MusicOpus_256)  { Weight = 314, MinSize = 0, MaxSize = 15, PreferredSize = 10 },
                new QualityDefinition(Quality.MusicWMA)       { Weight = 315, MinSize = 0, MaxSize = 15, PreferredSize = 10 },

                // Music quality definitions - FLAC (Weight 320-329, 24/96 is TARGET)
                new QualityDefinition(Quality.MusicFLAC_16_44) { Weight = 320, MinSize = 0, MaxSize = 60, PreferredSize = 40, GroupName = GroupFlacCd },
                new QualityDefinition(Quality.MusicFLAC_16_48) { Weight = 321, MinSize = 0, MaxSize = 65, PreferredSize = 45, GroupName = GroupFlacCd },
                new QualityDefinition(Quality.MusicFLAC_24_44) { Weight = 322, MinSize = 0, MaxSize = 90, PreferredSize = 60, GroupName = GroupFlacHiRes },
                new QualityDefinition(Quality.MusicFLAC_24_48) { Weight = 323, MinSize = 0, MaxSize = 100, PreferredSize = 65, GroupName = GroupFlacHiRes },
                new QualityDefinition(Quality.MusicFLAC_24_88) { Weight = 324, MinSize = 0, MaxSize = 180, PreferredSize = 120, GroupName = GroupFlacHiRes },
                new QualityDefinition(Quality.MusicFLAC_24_96) { Weight = 325, MinSize = 0, MaxSize = 200, PreferredSize = 130, GroupName = GroupFlacHiRes },
                new QualityDefinition(Quality.MusicFLAC_24_176) { Weight = 326, MinSize = 0, MaxSize = 400, PreferredSize = 250, GroupName = GroupFlacUltraHiRes },
                new QualityDefinition(Quality.MusicFLAC_24_192) { Weight = 327, MinSize = 0, MaxSize = 450, PreferredSize = 280, GroupName = GroupFlacUltraHiRes },

                // Music quality definitions - WAV (Weight 340-349)
                new QualityDefinition(Quality.MusicWAV_16_44)  { Weight = 340, MinSize = 0, MaxSize = 100, PreferredSize = 70, GroupName = GroupWavCd },
                new QualityDefinition(Quality.MusicWAV_16_48)  { Weight = 341, MinSize = 0, MaxSize = 110, PreferredSize = 75, GroupName = GroupWavCd },
                new QualityDefinition(Quality.MusicWAV_24_44)  { Weight = 342, MinSize = 0, MaxSize = 150, PreferredSize = 100, GroupName = GroupWavHiRes },
                new QualityDefinition(Quality.MusicWAV_24_48)  { Weight = 343, MinSize = 0, MaxSize = 165, PreferredSize = 110, GroupName = GroupWavHiRes },
                new QualityDefinition(Quality.MusicWAV_24_88)  { Weight = 344, MinSize = 0, MaxSize = 300, PreferredSize = 200, GroupName = GroupWavHiRes },
                new QualityDefinition(Quality.MusicWAV_24_96)  { Weight = 345, MinSize = 0, MaxSize = 330, PreferredSize = 220, GroupName = GroupWavHiRes },
                new QualityDefinition(Quality.MusicWAV_24_176) { Weight = 346, MinSize = 0, MaxSize = 600, PreferredSize = 400, GroupName = GroupWavUltraHiRes },
                new QualityDefinition(Quality.MusicWAV_24_192) { Weight = 347, MinSize = 0, MaxSize = 660, PreferredSize = 440, GroupName = GroupWavUltraHiRes },

                // Music quality definitions - AIFF (Weight 350-359)
                new QualityDefinition(Quality.MusicAIFF_16_44) { Weight = 350, MinSize = 0, MaxSize = 100, PreferredSize = 70, GroupName = GroupAiffCd },
                new QualityDefinition(Quality.MusicAIFF_16_48) { Weight = 351, MinSize = 0, MaxSize = 110, PreferredSize = 75, GroupName = GroupAiffCd },
                new QualityDefinition(Quality.MusicAIFF_24_44) { Weight = 352, MinSize = 0, MaxSize = 150, PreferredSize = 100, GroupName = GroupAiffHiRes },
                new QualityDefinition(Quality.MusicAIFF_24_48) { Weight = 353, MinSize = 0, MaxSize = 165, PreferredSize = 110, GroupName = GroupAiffHiRes },
                new QualityDefinition(Quality.MusicAIFF_24_88) { Weight = 354, MinSize = 0, MaxSize = 300, PreferredSize = 200, GroupName = GroupAiffHiRes },
                new QualityDefinition(Quality.MusicAIFF_24_96) { Weight = 355, MinSize = 0, MaxSize = 330, PreferredSize = 220, GroupName = GroupAiffHiRes },
                new QualityDefinition(Quality.MusicAIFF_24_176) { Weight = 356, MinSize = 0, MaxSize = 600, PreferredSize = 400, GroupName = GroupAiffUltraHiRes },
                new QualityDefinition(Quality.MusicAIFF_24_192) { Weight = 357, MinSize = 0, MaxSize = 660, PreferredSize = 440, GroupName = GroupAiffUltraHiRes },

                // Music quality definitions - DSD (Weight 360-369)
                new QualityDefinition(Quality.MusicDSD64)  { Weight = 360, MinSize = 0, MaxSize = 200, PreferredSize = 130, GroupName = GroupDsd },
                new QualityDefinition(Quality.MusicDSD128) { Weight = 361, MinSize = 0, MaxSize = 400, PreferredSize = 260, GroupName = GroupDsd },
                new QualityDefinition(Quality.MusicDSD256) { Weight = 362, MinSize = 0, MaxSize = 800, PreferredSize = 520, GroupName = GroupDsd },
                new QualityDefinition(Quality.MusicDSD512) { Weight = 363, MinSize = 0, MaxSize = null, PreferredSize = null, GroupName = GroupDsd },

                // Music quality definitions - Other Lossless (Weight 370-379)
                new QualityDefinition(Quality.MusicALAC_16_44) { Weight = 370, MinSize = 0, MaxSize = 60, PreferredSize = 40, GroupName = GroupAlacCd },
                new QualityDefinition(Quality.MusicALAC_16_48) { Weight = 371, MinSize = 0, MaxSize = 65, PreferredSize = 45, GroupName = GroupAlacCd },
                new QualityDefinition(Quality.MusicALAC_24_44) { Weight = 372, MinSize = 0, MaxSize = 90, PreferredSize = 60, GroupName = GroupAlacHiRes },
                new QualityDefinition(Quality.MusicALAC_24_48) { Weight = 373, MinSize = 0, MaxSize = 100, PreferredSize = 65, GroupName = GroupAlacHiRes },
                new QualityDefinition(Quality.MusicALAC_24_96) { Weight = 374, MinSize = 0, MaxSize = 200, PreferredSize = 130, GroupName = GroupAlacHiRes },
                new QualityDefinition(Quality.MusicALAC_24_192) { Weight = 375, MinSize = 0, MaxSize = 450, PreferredSize = 280, GroupName = GroupAlacUltraHiRes },
                new QualityDefinition(Quality.MusicAPE)        { Weight = 376, MinSize = 0, MaxSize = 60, PreferredSize = 40 },
                new QualityDefinition(Quality.MusicWavPack)    { Weight = 377, MinSize = 0, MaxSize = 60, PreferredSize = 40 },

                // Music quality definitions - Special (Weight 380-389)
                new QualityDefinition(Quality.MusicMQA)        { Weight = 380, MinSize = 0, MaxSize = 80, PreferredSize = 50, GroupName = GroupMqa },
                new QualityDefinition(Quality.MusicMQA_Studio) { Weight = 381, MinSize = 0, MaxSize = 100, PreferredSize = 60, GroupName = GroupMqa }
            };
        }

        public static readonly List<Quality> All;

        public static readonly Quality[] AllLookup;

        public static readonly HashSet<QualityDefinition> DefaultQualityDefinitions;
        public static Quality FindById(int id)
        {
            if (id == 0)
            {
                return Unknown;
            }

            var quality = AllLookup[id];

            if (quality == null)
            {
                throw new ArgumentException("ID does not match a known quality", "id");
            }

            return quality;
        }

        public static explicit operator Quality(int id)
        {
            return FindById(id);
        }

        public static explicit operator int(Quality quality)
        {
            return quality.Id;
        }
    }
}
