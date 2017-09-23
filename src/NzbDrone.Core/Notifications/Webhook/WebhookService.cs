using FluentValidation.Results;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Music;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Rest;
using RestSharp;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Parser.Model;
using System.Collections.Generic;

namespace NzbDrone.Core.Notifications.Webhook
{
    public interface IWebhookService
    {
        void OnDownload(Artist artist, TrackFile trackFile, WebhookSettings settings);
        void OnRename(Artist artist, WebhookSettings settings);
        void OnGrab(Artist artist, RemoteAlbum album, QualityModel quality, WebhookSettings settings);
        ValidationFailure Test(WebhookSettings settings);
    }

    public class WebhookService : IWebhookService
    {
        public void OnDownload(Artist artist, TrackFile trackFile, WebhookSettings settings)
        {
            var payload = new WebhookPayload
            {
                EventType = "Download",
                Artist = new WebhookArtist(artist),
                Albums = trackFile.Tracks.Value.ConvertAll(x => new WebhookAlbum(x.Album) {
                    Quality = trackFile.Quality.Quality.Name,
                    QualityVersion = trackFile.Quality.Revision.Version,
                    ReleaseGroup = trackFile.ReleaseGroup,
                    SceneName = trackFile.SceneName
                })
            };

            NotifyWebhook(payload, settings);
        }

        public void OnRename(Artist artist, WebhookSettings settings)
        {
            var payload = new WebhookPayload
            {
                EventType = "Rename",
                Artist = new WebhookArtist(artist)
            };

            NotifyWebhook(payload, settings);
        }

        public void OnGrab(Artist artist, RemoteAlbum album, QualityModel quality, WebhookSettings settings)
        {
            var payload = new WebhookPayload
            {
                EventType = "Grab",
                Artist = new WebhookArtist(artist),
                Albums = album.Albums.ConvertAll(x => new WebhookAlbum(x)
                {
                    Quality = quality.Quality.Name,
                    QualityVersion = quality.Revision.Version,
                    ReleaseGroup = album.ParsedAlbumInfo.ReleaseGroup
                })
            };
            NotifyWebhook(payload, settings);
        }

        public void NotifyWebhook(WebhookPayload body, WebhookSettings settings)
        {
            try {
                var client = RestClientFactory.BuildClient(settings.Url);
                var request = new RestRequest((Method) settings.Method);
                request.RequestFormat = DataFormat.Json;
                request.AddBody(body);
                client.ExecuteAndValidate(request);
            }
            catch (RestException ex)
            {
                throw new WebhookException("Unable to post to webhook: {0}", ex, ex.Message);
            }
        }

        public ValidationFailure Test(WebhookSettings settings)
        {
            try
            {
                NotifyWebhook(
                    new WebhookPayload
                    {
                        EventType = "Test",
                        Artist = new WebhookArtist()
                        {
                            Id = 1,
                            Title = "Test Title",
                            Path = "C:\\testpath",
                            MBId = "1234"
                        },
                        Albums = new List<WebhookAlbum>() {
                            new WebhookAlbum()
                            {
                                Id = 123,
                                Title = "Test title"
                            }
                        }
                    },
                    settings
                );
            }
            catch (WebhookException ex)
            {
                return new NzbDroneValidationFailure("Url", ex.Message);
            }

            return null;
        }
    }
}
