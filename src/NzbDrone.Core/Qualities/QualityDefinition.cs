using System.Collections.Generic;
using NzbDrone.Core.Datastore;


namespace NzbDrone.Core.Qualities
{
    public class QualityDefinition : ModelBase
    {
        // Quality will be preserved for legacy reasons, e.g. to identify old definitions and update them with the correct quality tags.
        public Quality Quality { get; set; }

        public string Title { get; set; }

        public int Weight { get; set; }

        public double? MinSize { get; set; }
        public double? MaxSize { get; set; }

        public List<QualityTag> QualityTags { get; set; }

        public QualityDefinition ParentQualityDefinition { get; set; }

        public QualityDefinition()
        {

        }

        public QualityDefinition(Quality quality)
        {
            Quality = quality;
            Title = quality.Name;
        }

        public override string ToString()
        {
            return Title;
        }

        public static readonly HashSet<QualityDefinition> DefaultQualityDefinitions = new HashSet<QualityDefinition>
            {
                new QualityDefinition(Quality.Unknown)     { Weight = 1,  MinSize = 0, MaxSize = 100, QualityTags = new List<QualityTag>() },
                new QualityDefinition(Quality.WORKPRINT)   { Weight = 2,  MinSize = 0, MaxSize = 100, QualityTags = new List<QualityTag>{ new QualityTag("s_workprint") } },
                new QualityDefinition(Quality.CAM)         { Weight = 3,  MinSize = 0, MaxSize = 100, QualityTags = new List<QualityTag>{ new QualityTag("s_cam") } },
                new QualityDefinition(Quality.TELESYNC)    { Weight = 4,  MinSize = 0, MaxSize = 100, QualityTags = new List<QualityTag>{ new QualityTag("s_telesync") } },
                new QualityDefinition(Quality.TELECINE)    { Weight = 5,  MinSize = 0, MaxSize = 100, QualityTags = new List<QualityTag>{ new QualityTag("s_telecine") } },
                new QualityDefinition(Quality.REGIONAL)    { Weight = 6,  MinSize = 0, MaxSize = 100, QualityTags = new List<QualityTag>{ new QualityTag("s_dvd"), new QualityTag("m_regional") } },
                new QualityDefinition(Quality.DVDSCR)      { Weight = 7,  MinSize = 0, MaxSize = 100, QualityTags = new List<QualityTag>{ new QualityTag("s_dvd"), new QualityTag("m_screener") } },
                new QualityDefinition(Quality.SDTV)        { Weight = 8,  MinSize = 0, MaxSize = 100, QualityTags = new List<QualityTag>{ new QualityTag("s_tv"), new QualityTag("r_480") } },
                new QualityDefinition(Quality.DVD)         { Weight = 9,  MinSize = 0, MaxSize = 100, QualityTags = new List<QualityTag>{ new QualityTag("s_dvd") } },
                new QualityDefinition(Quality.DVDR)        { Weight = 10,  MinSize = 0, MaxSize = 100, QualityTags = new List<QualityTag>{ } }, //TODO: Update dvd r quality

                new QualityDefinition(Quality.WEBDL480p)   { Weight = 11, MinSize = 0, MaxSize = 100, QualityTags = new List<QualityTag>{ new QualityTag("s_webdl"), new QualityTag("r_480") } },
                new QualityDefinition(Quality.Bluray480p)  { Weight = 12, MinSize = 0, MaxSize = 100, QualityTags = new List<QualityTag>{ new QualityTag("s_bluray"), new QualityTag("r_480") } },
                new QualityDefinition(Quality.Bluray576p)  { Weight = 13, MinSize = 0, MaxSize = 100, QualityTags = new List<QualityTag>{ new QualityTag("s_bluray"), new QualityTag("r_576") } },

                new QualityDefinition(Quality.HDTV720p)    { Weight = 14, MinSize = 0, MaxSize = 100, QualityTags = new List<QualityTag>{ new QualityTag("s_tv"), new QualityTag("r_720") } },
                new QualityDefinition(Quality.WEBDL720p)   { Weight = 15, MinSize = 0, MaxSize = 100, QualityTags = new List<QualityTag>{ new QualityTag("s_webdl"), new QualityTag("r_720") } },
                new QualityDefinition(Quality.Bluray720p)  { Weight = 16, MinSize = 0, MaxSize = 100, QualityTags = new List<QualityTag>{ new QualityTag("s_bluray"), new QualityTag("r_720") } },

                new QualityDefinition(Quality.HDTV1080p)   { Weight = 17, MinSize = 0, MaxSize = 100, QualityTags = new List<QualityTag>{ new QualityTag("s_tv"), new QualityTag("r_1080") } },
                new QualityDefinition(Quality.WEBDL1080p)  { Weight = 18, MinSize = 0, MaxSize = 100, QualityTags = new List<QualityTag>{ new QualityTag("S_webdl"), new QualityTag("R_1080") } },
                new QualityDefinition(Quality.Bluray1080p) { Weight = 19, MinSize = 0, MaxSize = null, QualityTags = new List<QualityTag>{ new QualityTag("S_bluray"), new QualityTag("R_1080") } },
                new QualityDefinition(Quality.Remux1080p)  { Weight = 20, MinSize = 0, MaxSize = null, QualityTags = new List<QualityTag>{ new QualityTag("s_bluray"), new QualityTag("R_1080"), new QualityTag("m_remux") } },

                new QualityDefinition(Quality.HDTV2160p)   { Weight = 21, MinSize = 0, MaxSize = null, QualityTags = new List<QualityTag>{ new QualityTag("s_tv"), new QualityTag("r_2160") } },
                new QualityDefinition(Quality.WEBDL2160p)  { Weight = 22, MinSize = 0, MaxSize = null, QualityTags = new List<QualityTag>{ new QualityTag("s_webdl"), new QualityTag("r_2160") } },
                new QualityDefinition(Quality.Bluray2160p) { Weight = 23, MinSize = 0, MaxSize = null, QualityTags = new List<QualityTag>{ new QualityTag("s_bluray"), new QualityTag("r_2160") } },
                new QualityDefinition(Quality.Remux2160p)  { Weight = 24, MinSize = 0, MaxSize = null, QualityTags = new List<QualityTag>{ new QualityTag("s_bluray"), new QualityTag("r_2160"), new QualityTag("m_remux") } },

                new QualityDefinition(Quality.BRDISK)      { Weight = 25, MinSize = 0, MaxSize = null, QualityTags = new List<QualityTag>{ new QualityTag("s_bluray"), new QualityTag("r_1080"), new QualityTag("r_2160"), new QualityTag("m_brdisk") } },
                new QualityDefinition(Quality.RAWHD)       { Weight = 26, MinSize = 0, MaxSize = null, QualityTags = new List<QualityTag>{ new QualityTag("s_tv"), new QualityTag("r_1080"), new QualityTag("r_2160"), new QualityTag("m_rawhd") } }
            };

    }
}
