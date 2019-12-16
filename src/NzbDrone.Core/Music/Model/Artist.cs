using Marr.Data;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Profiles.Qualities;
using NzbDrone.Core.Profiles.Metadata;
using System;
using System.Collections.Generic;
using Equ;

namespace NzbDrone.Core.Music
{
    public class Artist : Entity<Artist>
    {
        public Artist()
        {
            Tags = new HashSet<int>();
            Metadata = new ArtistMetadata();
        }

        // These correspond to columns in the Artists table
        public int ArtistMetadataId { get; set; }
        public string CleanName { get; set; }
        public string SortName { get; set; }
        public bool Monitored { get; set; }
        public bool AlbumFolder { get; set; }
        public DateTime? LastInfoSync { get; set; }
        public string Path { get; set; }
        public string RootFolderPath { get; set; }
        public DateTime Added { get; set; }
        public int QualityProfileId { get; set; }
        public int MetadataProfileId { get; set; }
        public HashSet<int> Tags { get; set; }
        [MemberwiseEqualityIgnore]
        public AddArtistOptions AddOptions { get; set; }

        // Dynamically loaded from DB
        [MemberwiseEqualityIgnore]
        public LazyLoaded<ArtistMetadata> Metadata { get; set; }
        [MemberwiseEqualityIgnore]
        public LazyLoaded<QualityProfile> QualityProfile { get; set; }
        [MemberwiseEqualityIgnore]
        public LazyLoaded<MetadataProfile> MetadataProfile { get; set; }
        [MemberwiseEqualityIgnore]
        public LazyLoaded<List<Album>> Albums { get; set; }

        //compatibility properties
        [MemberwiseEqualityIgnore]
        public string Name { get { return Metadata.Value.Name; } set { Metadata.Value.Name = value; } }
        [MemberwiseEqualityIgnore]
        public string ForeignArtistId { get { return Metadata.Value.ForeignArtistId; } set { Metadata.Value.ForeignArtistId = value; } }

        public override string ToString()
        {
            return string.Format("[{0}][{1}]", Metadata.Value.ForeignArtistId.NullSafe(), Metadata.Value.Name.NullSafe());
        }

        public override void UseMetadataFrom(Artist other)
        {
            CleanName = other.CleanName;
            SortName = other.SortName;
        }

        public override void UseDbFieldsFrom(Artist other)
        {
            Id = other.Id;
            ArtistMetadataId = other.ArtistMetadataId;
            Monitored = other.Monitored;
            AlbumFolder = other.AlbumFolder;
            LastInfoSync = other.LastInfoSync;
            Path = other.Path;
            RootFolderPath = other.RootFolderPath;
            Added = other.Added;
            QualityProfileId = other.QualityProfileId;
            MetadataProfileId = other.MetadataProfileId;
            Tags = other.Tags;
            AddOptions = other.AddOptions;
        }

        public override void ApplyChanges(Artist other)
        {
            Path = other.Path;
            QualityProfileId = other.QualityProfileId;
            QualityProfile = other.QualityProfile;
            MetadataProfileId = other.MetadataProfileId;
            MetadataProfile = other.MetadataProfile;

            Albums = other.Albums;
            Tags = other.Tags;
            AddOptions = other.AddOptions;
            RootFolderPath = other.RootFolderPath;
            Monitored = other.Monitored;
            AlbumFolder = other.AlbumFolder;
        }
    }
}
