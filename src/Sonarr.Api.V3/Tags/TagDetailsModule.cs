using NzbDrone.Core.Tags;
using Lidarr.Http;

namespace Lidarr.Api.V3.Tags
{
    public class TagDetailsModule : LidarrRestModule<TagDetailsResource>
    {
        private readonly ITagService _tagService;

        public TagDetailsModule(ITagService tagService)
            : base("/tag/details")
        {
            _tagService = tagService;

            GetResourceById = Get;
        }

        private TagDetailsResource Get(int id)
        {
            return _tagService.Details(id).ToResource();
        }
    }
}
