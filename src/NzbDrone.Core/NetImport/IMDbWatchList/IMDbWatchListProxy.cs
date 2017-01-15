using System;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using FluentValidation.Results;
using NLog;
using NzbDrone.Core.Exceptions;
using RestSharp;
using NzbDrone.Core.Rest;

namespace NzbDrone.Core.NetImport.IMDbWatchList
{
    public interface IIMDbWatchListProxy
    {
        void ImportMovies(string url);
        ValidationFailure Test(IMDbWatchListSettings settings);
    }

    public class IMDbWatchListProxy : IIMDbWatchListProxy
    {
        private readonly Logger _logger;
        private const string URL = "http://rss.imdb.com";

        public IMDbWatchListProxy(Logger logger)
        {
            _logger = logger;
        }

        public void ImportMovies(string id)
        {
            var client = RestClientFactory.BuildClient(URL);
            var request = new RestRequest("/list/{id}", Method.GET);
            request.RequestFormat = DataFormat.Xml;
            request.AddParameter("id", id, ParameterType.UrlSegment);

            var response = client.ExecuteAndValidate(request);
            ValidateResponse(response);
        }

        private void Verify(string id)
        {
            var client = RestClientFactory.BuildClient(URL);
            var request = new RestRequest("/list/{id}", Method.GET);
            request.RequestFormat = DataFormat.Xml;
            request.AddParameter("id", id, ParameterType.UrlSegment);

            var response = client.ExecuteAndValidate(request);
            ValidateResponse(response);
        }

        private void ValidateResponse(IRestResponse response)
        {
            var xDoc = XDocument.Parse(response.Content);
            var nma = xDoc.Descendants("nma").Single();
            var error = nma.Descendants("error").SingleOrDefault();

            if (error != null)
            {
                ((HttpStatusCode)Convert.ToInt32(error.Attribute("code").Value)).VerifyStatusCode(error.Value);
            }
        }

        public ValidationFailure Test(IMDbWatchListSettings settings)
        {
            try
            {
                Verify(settings.Link);
                ImportMovies(settings.Link);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unable to import movies: " + ex.Message);
                return new ValidationFailure("IMDbWatchListId", "Unable to import movies");
            }

            return null;
        }
    }
}
