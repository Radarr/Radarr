using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.ImportLists.Spotify;
using NzbDrone.Core.Test.Framework;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Models;

namespace NzbDrone.Core.Test.ImportListTests
{
    [TestFixture]
    public class SpotifySavedAlbumsFixture : CoreTest<SpotifySavedAlbums>
    {
        // placeholder, we don't use real API
        private readonly SpotifyWebAPI api = null;

        [Test]
        public void should_not_throw_if_saved_albums_is_null()
        {
            Mocker.GetMock<ISpotifyProxy>().
                Setup(x => x.GetSavedAlbums(It.IsAny<SpotifySavedAlbums>(),
                                                It.IsAny<SpotifyWebAPI>()))
                .Returns(default(Paging<SavedAlbum>));

            var result = Subject.Fetch(api);

            result.Should().BeEmpty();
        }

        [Test]
        public void should_not_throw_if_saved_album_items_is_null()
        {
            var savedAlbums = new Paging<SavedAlbum> {
                Items = null
            };

            Mocker.GetMock<ISpotifyProxy>().
                Setup(x => x.GetSavedAlbums(It.IsAny<SpotifySavedAlbums>(),
                                                It.IsAny<SpotifyWebAPI>()))
                .Returns(savedAlbums);

            var result = Subject.Fetch(api);

            result.Should().BeEmpty();
        }

        [Test]
        public void should_not_throw_if_saved_album_is_null()
        {
            var savedAlbums = new Paging<SavedAlbum> {
                Items = new List<SavedAlbum> {
                    null
                }
            };

            Mocker.GetMock<ISpotifyProxy>().
                Setup(x => x.GetSavedAlbums(It.IsAny<SpotifySavedAlbums>(),
                                                It.IsAny<SpotifyWebAPI>()))
                .Returns(savedAlbums);

            var result = Subject.Fetch(api);

            result.Should().BeEmpty();
        }

        [TestCase("Artist", "Album")]
        public void should_parse_saved_album(string artistName, string albumName)
        {
            var savedAlbums = new Paging<SavedAlbum> {
                Items = new List<SavedAlbum> {
                    new SavedAlbum {
                        Album = new FullAlbum {
                            Name = albumName,
                            Artists = new List<SimpleArtist> {
                                new SimpleArtist {
                                    Name = artistName
                                }
                            }
                        }
                    }
                }
            };

            Mocker.GetMock<ISpotifyProxy>().
                Setup(x => x.GetSavedAlbums(It.IsAny<SpotifySavedAlbums>(),
                                                It.IsAny<SpotifyWebAPI>()))
                .Returns(savedAlbums);

            var result = Subject.Fetch(api);

            result.Should().HaveCount(1);
        }

        [Test]
        public void should_not_throw_if_get_next_page_returns_null()
        {
            var savedAlbums = new Paging<SavedAlbum> {
                Items = new List<SavedAlbum> {
                    new SavedAlbum {
                        Album = new FullAlbum {
                            Name = "Album",
                            Artists = new List<SimpleArtist> {
                                new SimpleArtist {
                                    Name = "Artist"
                                }
                            }
                        }
                    }
                },
                Next = "DummyToMakeHasNextTrue"
            };

            Mocker.GetMock<ISpotifyProxy>().
                Setup(x => x.GetSavedAlbums(It.IsAny<SpotifySavedAlbums>(),
                                                It.IsAny<SpotifyWebAPI>()))
                .Returns(savedAlbums);

            Mocker.GetMock<ISpotifyProxy>()
                .Setup(x => x.GetNextPage(It.IsAny<SpotifyFollowedArtists>(),
                                          It.IsAny<SpotifyWebAPI>(),
                                          It.IsAny<Paging<SavedAlbum>>()))
                .Returns(default(Paging<SavedAlbum>));

            var result = Subject.Fetch(api);

            result.Should().HaveCount(1);

            Mocker.GetMock<ISpotifyProxy>()
                .Verify(x => x.GetNextPage(It.IsAny<SpotifySavedAlbums>(),
                                           It.IsAny<SpotifyWebAPI>(),
                                           It.IsAny<Paging<SavedAlbum>>()),
                        Times.Once());
        }

        [TestCase(null, "Album")]
        [TestCase("Artist", null)]
        [TestCase(null, null)]
        public void should_skip_bad_artist_or_album_names(string artistName, string albumName)
        {
            var savedAlbums = new Paging<SavedAlbum> {
                Items = new List<SavedAlbum> {
                    new SavedAlbum {
                        Album = new FullAlbum {
                            Name = albumName,
                            Artists = new List<SimpleArtist> {
                                new SimpleArtist {
                                    Name = artistName
                                }
                            }
                        }
                    }
                }
            };

            Mocker.GetMock<ISpotifyProxy>().
                Setup(x => x.GetSavedAlbums(It.IsAny<SpotifySavedAlbums>(),
                                                It.IsAny<SpotifyWebAPI>()))
                .Returns(savedAlbums);

            var result = Subject.Fetch(api);

            result.Should().BeEmpty();
        }
    }
}
