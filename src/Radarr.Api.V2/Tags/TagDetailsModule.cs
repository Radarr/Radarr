using System.Collections.Generic;
using NzbDrone.Core.Tags;
using Radarr.Http;

namespace Radarr.Api.V2.Tags
{
    public class TagDetailsModule : RadarrRestModule<TagDetailsResource>
    {
        private readonly ITagService _tagService;

        public TagDetailsModule(ITagService tagService)
            : base("/tag/detail")
        {
            _tagService = tagService;

            GetResourceById = GetById;
            GetResourceAll = GetAll;
        }

        private TagDetailsResource GetById(int id)
        {
            return _tagService.Details(id).ToResource();
        }

        private List<TagDetailsResource> GetAll()
        {
            return _tagService.Details().ToResource();
        }
    }
}
