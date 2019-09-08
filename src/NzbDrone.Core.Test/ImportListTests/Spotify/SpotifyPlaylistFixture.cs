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
    public class SpotifyPlaylistFixture : CoreTest<SpotifyPlaylist>
    {
        // placeholder, we don't use real API
        private readonly SpotifyWebAPI api = null;

        [Test]
        public void should_not_throw_if_playlist_tracks_is_null()
        {
            Mocker.GetMock<ISpotifyProxy>().
                Setup(x => x.GetPlaylistTracks(It.IsAny<SpotifyPlaylist>(),
                                               It.IsAny<SpotifyWebAPI>(),
                                               It.IsAny<string>(),
                                               It.IsAny<string>()))
                .Returns(default(Paging<PlaylistTrack>));

            var result = Subject.Fetch(api, "playlistid");

            result.Should().BeEmpty();
        }

        [Test]
        public void should_not_throw_if_playlist_tracks_items_is_null()
        {
            var playlistTracks = new Paging<PlaylistTrack> {
                Items = null
            };

            Mocker.GetMock<ISpotifyProxy>().
                Setup(x => x.GetPlaylistTracks(It.IsAny<SpotifyPlaylist>(),
                                               It.IsAny<SpotifyWebAPI>(),
                                               It.IsAny<string>(),
                                               It.IsAny<string>()))
                .Returns(playlistTracks);

            var result = Subject.Fetch(api, "playlistid");

            result.Should().BeEmpty();
        }

        [Test]
        public void should_not_throw_if_playlist_track_is_null()
        {
            var playlistTracks = new Paging<PlaylistTrack> {
                Items = new List<PlaylistTrack> {
                    null
                }
            };

            Mocker.GetMock<ISpotifyProxy>().
                Setup(x => x.GetPlaylistTracks(It.IsAny<SpotifyPlaylist>(),
                                               It.IsAny<SpotifyWebAPI>(),
                                               It.IsAny<string>(),
                                               It.IsAny<string>()))
                .Returns(playlistTracks);

            var result = Subject.Fetch(api, "playlistid");

            result.Should().BeEmpty();
        }

        [Test]
        public void should_use_album_artist_when_it_exists()
        {
            var playlistTracks = new Paging<PlaylistTrack> {
                Items = new List<PlaylistTrack> {
                    new PlaylistTrack {
                        Track = new FullTrack {
                            Album = new SimpleAlbum {
                                Name = "Album",
                                Artists = new List<SimpleArtist> {
                                    new SimpleArtist {
                                        Name = "AlbumArtist"
                                    }
                                }
                            },
                            Artists = new List<SimpleArtist> {
                                new SimpleArtist {
                                    Name = "TrackArtist"
                                }
                            }
                        }
                    }
                }
            };

            Mocker.GetMock<ISpotifyProxy>().
                Setup(x => x.GetPlaylistTracks(It.IsAny<SpotifyPlaylist>(),
                                               It.IsAny<SpotifyWebAPI>(),
                                               It.IsAny<string>(),
                                               It.IsAny<string>()))
                .Returns(playlistTracks);

            var result = Subject.Fetch(api, "playlistid");

            result.Should().HaveCount(1);
            result[0].Artist.Should().Be("AlbumArtist");
        }

        [Test]
        public void should_fall_back_to_track_artist_if_album_artist_missing()
        {
            var playlistTracks = new Paging<PlaylistTrack> {
                Items = new List<PlaylistTrack> {
                    new PlaylistTrack {
                        Track = new FullTrack {
                            Album = new SimpleAlbum {
                                Name = "Album",
                                Artists = new List<SimpleArtist> {
                                    new SimpleArtist {
                                        Name = null
                                    }
                                }
                            },
                            Artists = new List<SimpleArtist> {
                                new SimpleArtist {
                                    Name = "TrackArtist"
                                }
                            }
                        }
                    }
                }
            };

            Mocker.GetMock<ISpotifyProxy>().
                Setup(x => x.GetPlaylistTracks(It.IsAny<SpotifyPlaylist>(),
                                               It.IsAny<SpotifyWebAPI>(),
                                               It.IsAny<string>(),
                                               It.IsAny<string>()))
                .Returns(playlistTracks);

            var result = Subject.Fetch(api, "playlistid");

            result.Should().HaveCount(1);
            result[0].Artist.Should().Be("TrackArtist");
        }


        [TestCase(null, null, "Album")]
        [TestCase("AlbumArtist", null, null)]
        [TestCase(null, "TrackArtist", null)]
        public void should_skip_bad_artist_or_album_names(string albumArtistName, string trackArtistName, string albumName)
        {
            var playlistTracks = new Paging<PlaylistTrack> {
                Items = new List<PlaylistTrack> {
                    new PlaylistTrack {
                        Track = new FullTrack {
                            Album = new SimpleAlbum {
                                Name = albumName,
                                Artists = new List<SimpleArtist> {
                                    new SimpleArtist {
                                        Name = albumArtistName
                                    }
                                }
                            },
                            Artists = new List<SimpleArtist> {
                                new SimpleArtist {
                                    Name = trackArtistName
                                }
                            }
                        }
                    }
                }
            };

            Mocker.GetMock<ISpotifyProxy>().
                Setup(x => x.GetPlaylistTracks(It.IsAny<SpotifyPlaylist>(),
                                               It.IsAny<SpotifyWebAPI>(),
                                               It.IsAny<string>(),
                                               It.IsAny<string>()))
                .Returns(playlistTracks);

            var result = Subject.Fetch(api, "playlistid");

            result.Should().BeEmpty();
        }

        [Test]
        public void should_not_throw_if_get_next_page_returns_null()
        {
            var playlistTracks = new Paging<PlaylistTrack> {
                Items = new List<PlaylistTrack> {
                    new PlaylistTrack {
                        Track = new FullTrack {
                            Album = new SimpleAlbum {
                                Name = "Album",
                                Artists = new List<SimpleArtist> {
                                    new SimpleArtist {
                                        Name = null
                                    }
                                }
                            },
                            Artists = new List<SimpleArtist> {
                                new SimpleArtist {
                                    Name = "TrackArtist"
                                }
                            }
                        }
                    }
                },
                Next = "DummyToMakeHasNextTrue"
            };

            Mocker.GetMock<ISpotifyProxy>().
                Setup(x => x.GetPlaylistTracks(It.IsAny<SpotifyPlaylist>(),
                                               It.IsAny<SpotifyWebAPI>(),
                                               It.IsAny<string>(),
                                               It.IsAny<string>()))
                .Returns(playlistTracks);

            Mocker.GetMock<ISpotifyProxy>()
                .Setup(x => x.GetNextPage(It.IsAny<SpotifyFollowedArtists>(),
                                          It.IsAny<SpotifyWebAPI>(),
                                          It.IsAny<Paging<PlaylistTrack>>()))
                .Returns(default(Paging<PlaylistTrack>));

            var result = Subject.Fetch(api, "playlistid");

            result.Should().HaveCount(1);

            Mocker.GetMock<ISpotifyProxy>()
                .Verify(x => x.GetNextPage(It.IsAny<SpotifyPlaylist>(),
                                           It.IsAny<SpotifyWebAPI>(),
                                           It.IsAny<Paging<PlaylistTrack>>()),
                        Times.Once());
        }
    }
}
