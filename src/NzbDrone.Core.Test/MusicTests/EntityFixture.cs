using NUnit.Framework;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Music;
using NzbDrone.Test.Common;
using FluentAssertions;
using System.Collections;
using System.Reflection;
using AutoFixture;
using System.Linq;
using Equ;
using Marr.Data;

namespace NzbDrone.Core.Test.MusicTests
{
    [TestFixture]
    public class EntityFixture : LoggingTest
    {

        Fixture fixture = new Fixture();

        private static bool IsNotMarkedAsIgnore(PropertyInfo propertyInfo)
        {
            return !propertyInfo.GetCustomAttributes(typeof(MemberwiseEqualityIgnoreAttribute), true).Any();
        }

        public class EqualityPropertySource<T>
        {
            public static IEnumerable TestCases
            {
                get
                {
                    foreach (var property in typeof(T).GetProperties().Where(x => x.CanRead && x.CanWrite && IsNotMarkedAsIgnore(x)))
                    {
                        yield return new TestCaseData(property).SetName($"{{m}}_{property.Name}");
                    }
                }
            }
        }

        public class IgnoredPropertySource<T>
        {
            public static IEnumerable TestCases
            {
                get
                {
                    foreach (var property in typeof(T).GetProperties().Where(x => x.CanRead && x.CanWrite && !IsNotMarkedAsIgnore(x)))
                    {
                        yield return new TestCaseData(property).SetName($"{{m}}_{property.Name}");
                    }
                }
            }
        }

        [Test]
        public void two_equivalent_artist_metadata_should_be_equal()
        {
            var item1 = fixture.Create<ArtistMetadata>();
            var item2 = item1.JsonClone();

            item1.Should().NotBeSameAs(item2);
            item1.Should().Be(item2);
        }

        [Test, TestCaseSource(typeof(EqualityPropertySource<ArtistMetadata>), "TestCases")]
        public void two_different_artist_metadata_should_not_be_equal(PropertyInfo prop)
        {
            var item1 = fixture.Create<ArtistMetadata>();
            var item2 = item1.JsonClone();
            var different = fixture.Create<ArtistMetadata>();

            // make item2 different in the property under consideration
            var differentEntry = prop.GetValue(different);
            prop.SetValue(item2, differentEntry);

            item1.Should().NotBeSameAs(item2);
            item1.Should().NotBe(item2);
        }

        [Test]
        public void metadata_and_db_fields_should_replicate_artist_metadata()
        {
            var item1 = fixture.Create<ArtistMetadata>();
            var item2 = fixture.Create<ArtistMetadata>();

            item1.Should().NotBe(item2);

            item1.UseMetadataFrom(item2);
            item1.UseDbFieldsFrom(item2);
            item1.Should().Be(item2);
        }

        private Track GivenTrack()
        {
            return fixture.Build<Track>()
                .Without(x => x.AlbumRelease)
                .Without(x => x.ArtistMetadata)
                .Without(x => x.TrackFile)
                .Without(x => x.Artist)
                .Without(x => x.AlbumId)
                .Without(x => x.Album)
                .Create();
        }

        [Test]
        public void two_equivalent_track_should_be_equal()
        {
            var item1 = GivenTrack();
            var item2 = item1.JsonClone();

            item1.Should().NotBeSameAs(item2);
            item1.Should().Be(item2);
        }

        [Test, TestCaseSource(typeof(EqualityPropertySource<Track>), "TestCases")]
        public void two_different_tracks_should_not_be_equal(PropertyInfo prop)
        {
            var item1 = GivenTrack();
            var item2 = item1.JsonClone();
            var different = GivenTrack();

            // make item2 different in the property under consideration
            var differentEntry = prop.GetValue(different);
            prop.SetValue(item2, differentEntry);

            item1.Should().NotBeSameAs(item2);
            item1.Should().NotBe(item2);
        }

        [Test]
        public void metadata_and_db_fields_should_replicate_track()
        {
            var item1 = GivenTrack();
            var item2 = GivenTrack();

            item1.Should().NotBe(item2);

            item1.UseMetadataFrom(item2);
            item1.UseDbFieldsFrom(item2);
            item1.Should().Be(item2);
        }

        private AlbumRelease GivenAlbumRelease()
        {
            return fixture.Build<AlbumRelease>()
                .Without(x => x.Album)
                .Without(x => x.Tracks)
                .Create();
        }

        [Test]
        public void two_equivalent_album_releases_should_be_equal()
        {
            var item1 = GivenAlbumRelease();
            var item2 = item1.JsonClone();

            item1.Should().NotBeSameAs(item2);
            item1.Should().Be(item2);
        }

        [Test, TestCaseSource(typeof(EqualityPropertySource<AlbumRelease>), "TestCases")]
        public void two_different_album_releases_should_not_be_equal(PropertyInfo prop)
        {
            var item1 = GivenAlbumRelease();
            var item2 = item1.JsonClone();
            var different = GivenAlbumRelease();

            // make item2 different in the property under consideration
            var differentEntry = prop.GetValue(different);
            prop.SetValue(item2, differentEntry);

            item1.Should().NotBeSameAs(item2);
            item1.Should().NotBe(item2);
        }

        [Test]
        public void metadata_and_db_fields_should_replicate_release()
        {
            var item1 = GivenAlbumRelease();
            var item2 = GivenAlbumRelease();

            item1.Should().NotBe(item2);

            item1.UseMetadataFrom(item2);
            item1.UseDbFieldsFrom(item2);
            item1.Should().Be(item2);
        }

        private Album GivenAlbum()
        {
            return fixture.Build<Album>()
                .Without(x => x.ArtistMetadata)
                .Without(x => x.AlbumReleases)
                .Without(x => x.Artist)
                .Without(x => x.ArtistId)
                .Create();
        }

        [Test]
        public void two_equivalent_albums_should_be_equal()
        {
            var item1 = GivenAlbum();
            var item2 = item1.JsonClone();

            item1.Should().NotBeSameAs(item2);
            item1.Should().Be(item2);
        }

        [Test, TestCaseSource(typeof(EqualityPropertySource<Album>), "TestCases")]
        public void two_different_albums_should_not_be_equal(PropertyInfo prop)
        {
            var item1 = GivenAlbum();
            var item2 = item1.JsonClone();
            var different = GivenAlbum();

            // make item2 different in the property under consideration
            if (prop.PropertyType == typeof(bool))
            {
                prop.SetValue(item2, !(bool)prop.GetValue(item1));
            }
            else
            {
                prop.SetValue(item2, prop.GetValue(different));
            }

            item1.Should().NotBeSameAs(item2);
            item1.Should().NotBe(item2);
        }

        [Test]
        public void metadata_and_db_fields_should_replicate_album()
        {
            var item1 = GivenAlbum();
            var item2 = GivenAlbum();

            item1.Should().NotBe(item2);

            item1.UseMetadataFrom(item2);
            item1.UseDbFieldsFrom(item2);
            item1.Should().Be(item2);
        }

        private Artist GivenArtist()
        {
            return fixture.Build<Artist>()
                .With(x => x.Metadata, new LazyLoaded<ArtistMetadata>(fixture.Create<ArtistMetadata>()))
                .Without(x => x.QualityProfile)
                .Without(x => x.MetadataProfile)
                .Without(x => x.Albums)
                .Without(x => x.Name)
                .Without(x => x.ForeignArtistId)
                .Create();
        }

        [Test]
        public void two_equivalent_artists_should_be_equal()
        {
            var item1 = GivenArtist();
            var item2 = item1.JsonClone();

            item1.Should().NotBeSameAs(item2);
            item1.Should().Be(item2);
        }

        [Test, TestCaseSource(typeof(EqualityPropertySource<Artist>), "TestCases")]
        public void two_different_artists_should_not_be_equal(PropertyInfo prop)
        {
            var item1 = GivenArtist();
            var item2 = item1.JsonClone();
            var different = GivenArtist();

            // make item2 different in the property under consideration
            if (prop.PropertyType == typeof(bool))
            {
                prop.SetValue(item2, !(bool)prop.GetValue(item1));
            }
            else
            {
                prop.SetValue(item2, prop.GetValue(different));
            }

            item1.Should().NotBeSameAs(item2);
            item1.Should().NotBe(item2);
        }

        [Test]
        public void metadata_and_db_fields_should_replicate_artist()
        {
            var item1 = GivenArtist();
            var item2 = GivenArtist();

            item1.Should().NotBe(item2);

            item1.UseMetadataFrom(item2);
            item1.UseDbFieldsFrom(item2);
            item1.Should().Be(item2);
        }
    }
}
