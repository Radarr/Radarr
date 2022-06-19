using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using NzbDrone.Core.Notifications.Plex.PlexTv;

namespace Radarr.Http.Authentication.Plex
{
    public class PlexHandler : OAuthHandler<PlexOptions>
    {
        private readonly IPlexTvService _plexTvService;

        public PlexHandler(IOptionsMonitor<PlexOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, IPlexTvService plexTvService)
        : base(options, logger, encoder, clock)
        {
            _plexTvService = plexTvService;
        }

        protected override string BuildChallengeUrl(AuthenticationProperties properties, string redirectUri)
        {
            var pinUrl = _plexTvService.GetPinUrl();

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, pinUrl.Url);
            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = Backchannel.Send(requestMessage, Context.RequestAborted);
            var pin = JsonSerializer.Deserialize<PlexPinResponse>(response.Content.ReadAsStream());

            properties.Items.Add(PlexConstants.PinId, pin.id.ToString());

            var state = Options.StateDataFormat.Protect(properties);

            var plexRedirectUrl = QueryHelpers.AddQueryString(redirectUri, new Dictionary<string, string> { { "state", state } });

            return _plexTvService.GetSignInUrl(plexRedirectUrl, pin.id, pin.code).OauthUrl;
        }

        protected override async Task<HandleRequestResult> HandleRemoteAuthenticateAsync()
        {
            var query = Request.Query;

            var state = query["state"];
            var properties = Options.StateDataFormat.Unprotect(state);

            if (properties == null)
            {
                return HandleRequestResult.Fail("The oauth state was missing or invalid.");
            }

            if (!properties.Items.TryGetValue(PlexConstants.PinId, out var code))
            {
                return HandleRequestResult.Fail("The pin was missing or invalid.");
            }

            if (!int.TryParse(code, out var _))
            {
                return HandleRequestResult.Fail("The pin was in the wrong format.");
            }

            var codeExchangeContext = new OAuthCodeExchangeContext(properties, code, BuildRedirectUri(Options.CallbackPath));
            using var tokens = await ExchangeCodeAsync(codeExchangeContext);

            if (tokens.Error != null)
            {
                return HandleRequestResult.Fail(tokens.Error);
            }

            if (string.IsNullOrEmpty(tokens.AccessToken))
            {
                return HandleRequestResult.Fail("Failed to retrieve access token.");
            }

            var resources = _plexTvService.GetResources(tokens.AccessToken);

            var identity = new ClaimsIdentity(ClaimsIssuer);

            foreach (var resource in resources)
            {
                if (resource.Owned)
                {
                    identity.AddClaim(new Claim(PlexConstants.ServerOwnedClaim, resource.ClientIdentifier));
                }
                else
                {
                    identity.AddClaim(new Claim(PlexConstants.ServerAccessClaim, resource.ClientIdentifier));
                }
            }

            var ticket = await CreateTicketAsync(identity, properties, tokens);
            if (ticket != null)
            {
                return HandleRequestResult.Success(ticket);
            }
            else
            {
                return HandleRequestResult.Fail("Failed to retrieve user information from remote server.");
            }
        }

        protected override Task<OAuthTokenResponse> ExchangeCodeAsync(OAuthCodeExchangeContext context)
        {
            var token = _plexTvService.GetAuthToken(int.Parse(context.Code));

            var result = !StringValues.IsNullOrEmpty(token) switch
            {
                true => OAuthTokenResponse.Success(JsonDocument.Parse(string.Format("{{\"access_token\": \"{0}\"}}", token))),
                false => OAuthTokenResponse.Failed(new Exception("No token returned"))
            };

            return Task.FromResult(result);
        }

        private static OAuthTokenResponse PrepareFailedOAuthTokenReponse(HttpResponseMessage response, string body)
        {
            var errorMessage = $"OAuth token endpoint failure: Status: {response.StatusCode};Headers: {response.Headers};Body: {body};";
            return OAuthTokenResponse.Failed(new Exception(errorMessage));
        }

        private class PlexPinResponse
        {
            public int id { get; set; }
            public string code { get; set; }
        }
    }
}
