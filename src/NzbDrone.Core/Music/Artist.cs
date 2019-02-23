using Marr.Data;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Profiles.Languages;
using NzbDrone.Core.Profiles.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Music
{
    public class Artist : ModelBase
    {
        public Artist()
        {
            Tags = new HashSet<int>();
            Metadata = new ArtistMetadata();
        }

        public int ArtistMetadataId { get; set; }
        public LazyLoaded<ArtistMetadata> Metadata { get; set; }
        public string CleanName { get; set; }
        public string SortName { get; set; }
        public bool Monitored { get; set; }
        public bool AlbumFolder { get; set; }
        public DateTime? LastInfoSync { get; set; }
        public string Path { get; set; }
        public string RootFolderPath { get; set; }
        public DateTime Added { get; set; }
        public int QualityProfileId { get; set; }
        public LazyLoaded<QualityProfile> QualityProfile { get; set; }
        public int LanguageProfileId { get; set; }        
        public LazyLoaded<LanguageProfile> LanguageProfile { get; set; }
        public int MetadataProfileId { get; set; }        
        public LazyLoaded<MetadataProfile> MetadataProfile { get; set; }
        public LazyLoaded<List<Album>> Albums { get; set; }
        public HashSet<int> Tags { get; set; }
        public AddArtistOptions AddOptions { get; set; }

        public override string ToString()
        {
            return string.Format("[{0}][{1}]", Metadata.Value.ForeignArtistId, Metadata.Value.Name.NullSafe());
        }

        public void ApplyChanges(Artist otherArtist)
        {

            Path = otherArtist.Path;
            QualityProfileId = otherArtist.QualityProfileId;
            QualityProfile = otherArtist.QualityProfile;
            LanguageProfileId = otherArtist.LanguageProfileId;
            MetadataProfileId = otherArtist.MetadataProfileId;

            Albums = otherArtist.Albums;
            Tags = otherArtist.Tags;
            AddOptions = otherArtist.AddOptions;
            RootFolderPath = otherArtist.RootFolderPath;
            Monitored = otherArtist.Monitored;
            AlbumFolder = otherArtist.AlbumFolder;

        }

        //compatibility properties
        public string Name { get { return Metadata.Value.Name; } set { Metadata.Value.Name = value; } }
        public string ForeignArtistId { get { return Metadata.Value.ForeignArtistId; } set { Metadata.Value.ForeignArtistId = value; } }

    }
}
