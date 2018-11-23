using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Notifications;
using NzbDrone.Core.Profiles.Delay;
using NzbDrone.Core.Restrictions;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Tags
{
    public interface ITagService
    {
        Tag GetTag(int tagId);
        Tag GetTag(string tag);
        List<Tag> GetTags(IEnumerable<int> ids);
        TagDetails Details(int tagId);
        List<TagDetails> Details();
        List<Tag> All();
        Tag Add(Tag tag);
        Tag Update(Tag tag);
        void Delete(int tagId);
    }

    public class TagService : ITagService
    {
        private readonly ITagRepository _repo;
        private readonly IEventAggregator _eventAggregator;
        private readonly IDelayProfileService _delayProfileService;
        private readonly INotificationFactory _notificationFactory;
        private readonly IRestrictionService _restrictionService;
        private readonly IMovieService _movieService;

        public TagService(ITagRepository repo,
                          IEventAggregator eventAggregator,
                          IDelayProfileService delayProfileService,
                          INotificationFactory notificationFactory,
                          IRestrictionService restrictionService,
                          IMovieService movieService)
        {
            _repo = repo;
            _eventAggregator = eventAggregator;
            _delayProfileService = delayProfileService;
            _notificationFactory = notificationFactory;
            _restrictionService = restrictionService;
            _movieService = movieService;
        }

        public Tag GetTag(int tagId)
        {
            return _repo.Get(tagId);
        }

        public Tag GetTag(string tag)
        {
            if (tag.All(char.IsDigit))
            {
                return _repo.Get(int.Parse(tag));
            }
            else
            {
                return _repo.GetByLabel(tag);
            }
        }

        public List<Tag> GetTags(IEnumerable<int> ids)
        {
            return _repo.Get(ids).ToList();
        }

        public TagDetails Details(int tagId)
        {
            var tag = GetTag(tagId);
            var delayProfiles = _delayProfileService.AllForTag(tagId);
            var notifications = _notificationFactory.AllForTag(tagId);
            var restrictions = _restrictionService.AllForTag(tagId);
            var movies = _movieService.AllForTag(tagId);

            return new TagDetails
            {
                Id = tagId,
                Label = tag.Label,
                DelayProfileIds = delayProfiles.Select(c => c.Id).ToList(),
                NotificationIds = notifications.Select(c => c.Id).ToList(),
                RestrictionIds = restrictions.Select(c => c.Id).ToList(),
                MovieIds = movies.Select(c => c.Id).ToList()
            };
        }

        public List<TagDetails> Details()
        {
            var tags = All();
            var delayProfiles = _delayProfileService.All();
            var notifications = _notificationFactory.All();
            var restrictions = _restrictionService.All();
            var movies = _movieService.GetAllMovies();

            var details = new List<TagDetails>();

            foreach (var tag in tags)
            {
                details.Add(new TagDetails
                {
                    Id = tag.Id,
                    Label = tag.Label,
                    DelayProfileIds = delayProfiles.Where(c => c.Tags.Contains(tag.Id)).Select(c => c.Id).ToList(),
                    NotificationIds = notifications.Where(c => c.Tags.Contains(tag.Id)).Select(c => c.Id).ToList(),
                    RestrictionIds = restrictions.Where(c => c.Tags.Contains(tag.Id)).Select(c => c.Id).ToList(),
                    MovieIds = movies.Where(c => c.Tags.Contains(tag.Id)).Select(c => c.Id).ToList()
                }
                );
            }

            return details;
        }

        public List<Tag> All()
        {
            return _repo.All().OrderBy(t => t.Label).ToList();
        }

        public Tag Add(Tag tag)
        {
            //TODO: check for duplicate tag by label and return that tag instead?

            tag.Label = tag.Label.ToLowerInvariant();

            _repo.Insert(tag);
            _eventAggregator.PublishEvent(new TagsUpdatedEvent());

            return tag;
        }

        public Tag Update(Tag tag)
        {
            tag.Label = tag.Label.ToLowerInvariant();

            _repo.Update(tag);
            _eventAggregator.PublishEvent(new TagsUpdatedEvent());

            return tag;
        }

        public void Delete(int tagId)
        {
            _repo.Delete(tagId);
            _eventAggregator.PublishEvent(new TagsUpdatedEvent());
        }
    }
}
