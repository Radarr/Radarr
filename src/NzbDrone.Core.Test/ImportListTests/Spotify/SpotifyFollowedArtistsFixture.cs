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
    public class SpotifyFollowedArtistsFixture : CoreTest<SpotifyFollowedArtists>
    {
        // placeholder, we don't use real API
        private readonly SpotifyWebAPI api = null;

        [Test]
        public void should_not_throw_if_followed_is_null()
        {
            Mocker.GetMock<ISpotifyProxy>().
                Setup(x => x.GetFollowedArtists(It.IsAny<SpotifyFollowedArtists>(),
                                                It.IsAny<SpotifyWebAPI>()))
                .Returns(default(FollowedArtists));

            var result = Subject.Fetch(api);

            result.Should().BeEmpty();
        }

        [Test]
        public void should_not_throw_if_followed_artists_is_null()
        {
            var followed = new FollowedArtists {
                Artists = null
            };

            Mocker.GetMock<ISpotifyProxy>().
                Setup(x => x.GetFollowedArtists(It.IsAny<SpotifyFollowedArtists>(),
                                                It.IsAny<SpotifyWebAPI>()))
                .Returns(followed);

            var result = Subject.Fetch(api);

            result.Should().BeEmpty();
        }

        [Test]
        public void should_not_throw_if_followed_artist_items_is_null()
        {
            var followed = new FollowedArtists {
                Artists = new CursorPaging<FullArtist> {
                    Items = null
                }
            };

            Mocker.GetMock<ISpotifyProxy>().
                Setup(x => x.GetFollowedArtists(It.IsAny<SpotifyFollowedArtists>(),
                                                It.IsAny<SpotifyWebAPI>()))
                .Returns(followed);

            var result = Subject.Fetch(api);

            result.Should().BeEmpty();
            Subject.Fetch(api);
        }

        [Test]
        public void should_not_throw_if_artist_is_null()
        {
            var followed = new FollowedArtists {
                Artists = new CursorPaging<FullArtist> {
                    Items = new List<FullArtist> {
                        null
                    }
                }
            };

            Mocker.GetMock<ISpotifyProxy>().
                Setup(x => x.GetFollowedArtists(It.IsAny<SpotifyFollowedArtists>(),
                                                It.IsAny<SpotifyWebAPI>()))
                .Returns(followed);

            var result = Subject.Fetch(api);

            result.Should().BeEmpty();
            Subject.Fetch(api);
        }

        [Test]
        public void should_parse_followed_artist()
        {
            var followed = new FollowedArtists {
                Artists = new CursorPaging<FullArtist> {
                    Items = new List<FullArtist> {
                        new FullArtist {
                            Name = "artist"
                        }
                    }
                }
            };

            Mocker.GetMock<ISpotifyProxy>().
                Setup(x => x.GetFollowedArtists(It.IsAny<SpotifyFollowedArtists>(),
                                                It.IsAny<SpotifyWebAPI>()))
                .Returns(followed);

            var result = Subject.Fetch(api);

            result.Should().HaveCount(1);
        }

        [Test]
        public void should_not_throw_if_get_next_page_returns_null()
        {
            var followed = new FollowedArtists {
                Artists = new CursorPaging<FullArtist> {
                    Items = new List<FullArtist> {
                        new FullArtist {
                            Name = "artist"
                        }
                    },
                    Next = "DummyToMakeHasNextTrue"
                }
            };

            Mocker.GetMock<ISpotifyProxy>().
                Setup(x => x.GetFollowedArtists(It.IsAny<SpotifyFollowedArtists>(),
                                                It.IsAny<SpotifyWebAPI>()))
                .Returns(followed);

            Mocker.GetMock<ISpotifyProxy>()
                .Setup(x => x.GetNextPage(It.IsAny<SpotifyFollowedArtists>(),
                                          It.IsAny<SpotifyWebAPI>(),
                                          It.IsAny<FollowedArtists>()))
                .Returns(default(FollowedArtists));

            var result = Subject.Fetch(api);

            result.Should().HaveCount(1);

            Mocker.GetMock<ISpotifyProxy>()
                .Verify(v => v.GetNextPage(It.IsAny<SpotifyFollowedArtists>(),
                                           It.IsAny<SpotifyWebAPI>(),
                                           It.IsAny<FollowedArtists>()),
                        Times.Once());
        }

        [TestCase(null)]
        [TestCase("")]
        public void should_skip_bad_artist_names(string name)
        {
            var followed = new FollowedArtists {
                Artists = new CursorPaging<FullArtist> {
                    Items = new List<FullArtist> {
                        new FullArtist {
                            Name = name
                        }
                    }
                }
            };

            Mocker.GetMock<ISpotifyProxy>().
                Setup(x => x.GetFollowedArtists(It.IsAny<SpotifyFollowedArtists>(),
                                                It.IsAny<SpotifyWebAPI>()))
                .Returns(followed);

            var result = Subject.Fetch(api);

            result.Should().BeEmpty();
        }
    }
}
