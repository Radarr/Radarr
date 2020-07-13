using NzbDrone.Common.Http;

namespace NzbDrone.Core.NetImport.RadarrList2.StevenLu
{
    public class StevenLu2RequestGenerator : RadarrList2RequestGeneratorBase
    {
        public StevenLu2Settings Settings { get; set; }

        protected override HttpRequest GetHttpRequest()
        {
            var builder = RequestBuilder.Create()
                .SetSegment("route", $"list/stevenlu");

            if (Settings.Source != (int)StevenLuSource.Standard)
            {
                var source = ((StevenLuSource)Settings.Source).ToString().ToLower();

                var minScore = Settings.Source == (int)StevenLuSource.Imdb ? Settings.MinScore : Settings.MinScore * 10;
                builder.Resource($"{source}/{minScore}");
            }

            return builder.Build();
        }
    }
}
