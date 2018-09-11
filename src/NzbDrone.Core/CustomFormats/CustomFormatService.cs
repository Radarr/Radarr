﻿using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Composition;
using NzbDrone.Core.Blacklisting;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.History;
using NzbDrone.Core.Lifecycle;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Profiles;

namespace NzbDrone.Core.CustomFormats
{
    public interface ICustomFormatService
    {
        void Update(CustomFormat customFormat);
        CustomFormat Insert(CustomFormat customFormat);
        List<CustomFormat> All();
        CustomFormat GetById(int id);
        void Delete(int id);
    }


    public class CustomFormatService : ICustomFormatService, IHandle<ApplicationStartedEvent>
    {
        private readonly ICustomFormatRepository _formatRepository;
        private IProfileService _profileService;
        private readonly IMediaFileService _mediaFileService;
        private readonly IBlacklistService _blacklistService;
        private readonly IHistoryService _historyService;
        private readonly IPendingReleaseService _pendingReleaseService;

        public IProfileService ProfileService
        {
            get
            {
                if (_profileService == null)
                {
                    _profileService = _container.Resolve<IProfileService>();
                }

                return _profileService;
            }
        }

        private readonly IContainer _container;
        private readonly ICached<Dictionary<int, CustomFormat>> _cache;
        private readonly Logger _logger;

        public static Dictionary<int, CustomFormat> AllCustomFormats;

        public CustomFormatService(ICustomFormatRepository formatRepository, ICacheManager cacheManager,
            IContainer container, IHistoryService historyService,/*IMediaFileService mediaFileService, IBlacklistService blacklistService,
            IHistoryService historyService, IPendingReleaseService pendingReleaseService,*/
            Logger logger)
        {
            _formatRepository = formatRepository;
            _container = container;
            _cache = cacheManager.GetCache<Dictionary<int, CustomFormat>>(typeof(CustomFormat), "formats");
            /*_mediaFileService = mediaFileService;
            _blacklistService = blacklistService;
            _historyService = historyService;
            _pendingReleaseService = pendingReleaseService;*/
            _historyService = historyService;
            _logger = logger;
        }

        public void Update(CustomFormat customFormat)
        {
            _formatRepository.Update(customFormat);
            _cache.Clear();
        }

        public CustomFormat Insert(CustomFormat customFormat)
        {
            var ret = _formatRepository.Insert(customFormat);
            try
            {
                ProfileService.AddCustomFormat(ret);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failure while trying to add the new custom format to all profiles. Deleting again!");
                _formatRepository.Delete(ret);
                throw;
            }
            _cache.Clear();
            return ret;
        }

        public void Delete(int id)
        {
            _cache.Clear();
            try
            {
                //First history:
                var historyRepo = _container.Resolve<IHistoryRepository>();
                DeleteInRepo(historyRepo, h => h.Quality.CustomFormats, (h, f) =>
                {
                    h.Quality.CustomFormats = f;
                    return h;
                }, id);

                //Then Blacklist:
                var blacklistRepo = _container.Resolve<IBlacklistRepository>();
                DeleteInRepo(blacklistRepo, h => h.Quality.CustomFormats, (h, f) =>
                {
                    h.Quality.CustomFormats = f;
                    return h;
                }, id);

                //Then MovieFiles:
                var moviefileRepo = _container.Resolve<IMediaFileRepository>();
                DeleteInRepo(moviefileRepo, h => h.Quality.CustomFormats, (h, f) =>
                {
                    h.Quality.CustomFormats = f;
                    return h;
                }, id);

                //Then Profiles
                ProfileService.DeleteCustomFormat(id);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to delete format with id {} from other repositories! Format will not be deleted!", id);
                throw;
            }

            //Finally delete the format for real!
            _formatRepository.Delete(id);

            _cache.Clear();
        }

        private void DeleteInRepo<TModel>(IBasicRepository<TModel> repository, Func<TModel, List<CustomFormat>> queryFunc,
            Func<TModel, List<CustomFormat>, TModel> updateFunc, int customFormatId) where TModel : ModelBase, new()
        {
            var pagingSpec = new PagingSpec<TModel>
            {
                Page = 0,
                PageSize = 2000
            };
            while (true)
            {
                var allItems = repository.GetPaged(pagingSpec);
                var toUpdate = allItems.Records.Where(r => queryFunc(r).Exists(c => c.Id == customFormatId)).Select(r =>
                {
                    return updateFunc(r, queryFunc(r).Where(c => c.Id != customFormatId).ToList());
                });
                repository.UpdateMany(toUpdate.ToList());

                if (pagingSpec.Page * pagingSpec.PageSize >= allItems.TotalRecords)
                {
                    break;
                }

                pagingSpec.Page += 1;
            }
        }

        private Dictionary<int, CustomFormat> AllDictionary()
        {
            return _cache.Get("all", () =>
            {
                var all = _formatRepository.All().ToDictionary(m => m.Id);
                AllCustomFormats = all;
                return all;
            });
        }

        public List<CustomFormat> All()
        {
            return AllDictionary().Values.ToList();
        }

        public CustomFormat GetById(int id)
        {
            return AllDictionary()[id];
        }

        public static Dictionary<string, List<CustomFormat>> Templates
        {
            get
            {
                return new Dictionary<string, List<CustomFormat>>
                {
                    {
                        "Easy", new List<CustomFormat>
                        {
                            new CustomFormat("x264", "C_R_(x|h)264"),
                            new CustomFormat("x265", "C_R_(((x|h)265)|(HEVC))"),
                            new CustomFormat("Simple Hardcoded Subs", "C_R_subs?"),
                            new CustomFormat("Multi Language", "L_English", "L_French")
                        }
                    },
                    {
                        "Intermediate", new List<CustomFormat>
                        {
                            new CustomFormat("Hardcoded Subs", @"C_R_\b(?<hcsub>(\w+SUBS?)\b)|(?<hc>(HC|SUBBED))\b"),
                            new CustomFormat("Surround", @"C_R_\b((7|5).1)\b"),
                            new CustomFormat("Preferred Words", @"C_R_\b(SPARKS|Framestor)\b"),
                            new CustomFormat("Scene", @"I_G_Scene"),
                            new CustomFormat("Internal Releases", @"I_HDB_Internal", @"I_AHD_Internal")
                        }
                    },
                    {
                        "Advanced", new List<CustomFormat>
                        {
                            new CustomFormat("Custom")
                        }
                    }
                };
            }
        }

        public void Handle(ApplicationStartedEvent message)
        {
            // Fillup cache for DataMapper.
            All();
        }
    }
}
