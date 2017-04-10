using FluentValidation.Results;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Tv;
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
        void OnDownload(Movie movie, MovieFile movieFile, WebhookSettings settings);
        void OnRename(Movie movie, WebhookSettings settings);
        void OnGrab(Movie movie, RemoteMovie remoteMovie, QualityModel quality, WebhookSettings settings);
        ValidationFailure Test(WebhookSettings settings);
    }

    public class WebhookService : IWebhookService
    {
        public void OnDownload(Movie movie, MovieFile movieFile, WebhookSettings settings)
        {
            var payload = new WebhookPayload
            {
                EventType = "Download",
                Movie = new WebhookMovie(movie, movieFile),
                RemoteMovie = new WebhookRemoteMovie(movie)
            };

            NotifyWebhook(payload, settings);
        }

        public void OnRename(Movie movie, WebhookSettings settings)
        {
            var payload = new WebhookPayload
            {
                EventType = "Rename",
                Movie = new WebhookMovie(movie)
            };

            NotifyWebhook(payload, settings);
        }

        public void OnGrab(Movie movie, RemoteMovie remoteMovie, QualityModel quality, WebhookSettings settings)
        {
            var payload = new WebhookPayload
            {
                EventType = "Grab",
                Movie = new WebhookMovie(movie),
                RemoteMovie = new WebhookRemoteMovie(remoteMovie)
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
                        Movie = new WebhookMovie()
                        {
                            Id = 1,
                            Title = "Test Title",
                            FilePath = "C:\\testpath",
                        },
                        RemoteMovie = new WebhookRemoteMovie(){
                            ImdbId = "tt012345",
                        	Title = "My Awesome Movie!"
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
