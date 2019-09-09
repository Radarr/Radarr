using System.Collections.Generic;
using NzbDrone.Core.Tags;
using Lidarr.Http;

namespace Lidarr.Api.V1.Tags
{
    public class TagDetailsModule : LidarrRestModule<TagDetailsResource>
    {
        private readonly ITagService _tagService;

        public TagDetailsModule(ITagService tagService)
            : base("/tag/detail")
        {
            _tagService = tagService;

            GetResourceById = GetTagDetails;
            GetResourceAll = GetAll;
        }

        private TagDetailsResource GetTagDetails(int id)
        {
            return _tagService.Details(id).ToResource();
        }

        private List<TagDetailsResource> GetAll()
        {
            var tags =  _tagService.Details().ToResource();

            return tags;
        }
    }
}
