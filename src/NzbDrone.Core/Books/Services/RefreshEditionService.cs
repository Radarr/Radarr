using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Core.Books
{
    public interface IRefreshEditionService
    {
        bool RefreshEditionInfo(List<Edition> add, List<Edition> update, List<Tuple<Edition, Edition>> merge, List<Edition> delete, List<Edition> upToDate, List<Edition> remoteEditions, bool forceUpdateFileTags);
    }

    public class RefreshEditionService : IRefreshEditionService
    {
        private readonly IEditionService _editionService;
        private readonly IAudioTagService _audioTagService;
        private readonly Logger _logger;

        public RefreshEditionService(IEditionService editionService,
                                   IAudioTagService audioTagService,
                                   Logger logger)
        {
            _editionService = editionService;
            _audioTagService = audioTagService;
            _logger = logger;
        }

        public bool RefreshEditionInfo(List<Edition> add, List<Edition> update, List<Tuple<Edition, Edition>> merge, List<Edition> delete, List<Edition> upToDate, List<Edition> remoteEditions, bool forceUpdateFileTags)
        {
            var updateList = new List<Edition>();

            // for editions that need updating, just grab the remote edition and set db ids
            foreach (var edition in update)
            {
                var remoteEdition = remoteEditions.Single(e => e.ForeignEditionId == edition.ForeignEditionId);
                edition.UseMetadataFrom(remoteEdition);

                // make sure title is not null
                edition.Title = edition.Title ?? "Unknown";
                updateList.Add(edition);
            }

            _editionService.DeleteMany(delete.Concat(merge.Select(x => x.Item1)).ToList());
            _editionService.UpdateMany(updateList);

            var tagsToUpdate = updateList;
            if (forceUpdateFileTags)
            {
                _logger.Debug("Forcing tag update due to Author/Book/Edition updates");
                tagsToUpdate = updateList.Concat(upToDate).ToList();
            }

            _audioTagService.SyncTags(tagsToUpdate);

            return add.Any() || delete.Any() || updateList.Any() || merge.Any();
        }
    }
}
