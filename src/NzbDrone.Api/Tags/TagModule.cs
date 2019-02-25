using System.Collections.Generic;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tags;
using NzbDrone.SignalR;
using Radarr.Http;

namespace NzbDrone.Api.Tags
{
    public class TagModule : RadarrRestModuleWithSignalR<TagResource, Tag>, IHandle<TagsUpdatedEvent>
    {
        private readonly ITagService _tagService;

        public TagModule(IBroadcastSignalRMessage signalRBroadcaster,
                         ITagService tagService)
            : base(signalRBroadcaster)
        {
            _tagService = tagService;

            GetResourceById = GetTag;
            GetResourceAll = GetAllTags;
            CreateResource = CreateTag;
            UpdateResource = UpdateTag;
            DeleteResource = DeleteTag;
        }

        private TagResource GetTag(int id)
        {
            return _tagService.GetTag(id).ToResource();
        }

        private List<TagResource> GetAllTags()
        {
            return _tagService.All().ToResource();
        }

        private int CreateTag(TagResource resource)
        {
            var model = resource.ToModel();

            return _tagService.Add(model).Id;
        }

        private void UpdateTag(TagResource resource)
        {
            var model = resource.ToModel();

            _tagService.Update(model);
        }

        private void DeleteTag(int id)
        {
            _tagService.Delete(id);
        }

        public void Handle(TagsUpdatedEvent message)
        {
            BroadcastResourceChange(ModelAction.Sync);
        }
    }
}
