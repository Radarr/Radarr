using NzbDrone.Core.Movies.AlternativeTitles;
using Radarr.Http;

namespace NzbDrone.Api.Movies
{
    public class AlternativeTitleModule : RadarrRestModule<AlternativeTitleResource>
    {
        private readonly IAlternativeTitleService _altTitleService;

        public AlternativeTitleModule(IAlternativeTitleService altTitleService)
            : base("/alttitle")
        {
            _altTitleService = altTitleService;
            GetResourceById = GetTitle;
        }

        private AlternativeTitleResource GetTitle(int id)
        {
            return _altTitleService.GetById(id).ToResource();
        }
    }
}
