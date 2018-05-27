using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Composition;
using NzbDrone.Core.Lifecycle;
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
    }


    public class CustomFormatService : ICustomFormatService, IHandle<ApplicationStartedEvent>
    {
        private readonly ICustomFormatRepository _formatRepository;
        private IProfileService _profileService;

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
            IContainer container,
            Logger logger)
        {
            _formatRepository = formatRepository;
            _container = container;
            _cache = cacheManager.GetCache<Dictionary<int, CustomFormat>>(typeof(CustomFormat), "formats");
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
                _logger.Error("Failure while trying to add the new custom format to all profiles.", e);
                _formatRepository.Delete(ret);
                throw;
            }
            _cache.Clear();
            return ret;
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
                            new CustomFormat("Simple Hardcoded Subs", "C_R_subs?"),
                            new CustomFormat("Multi Language", "L_English", "L_French")
                        }
                    },
                    {
                        "Intermediate", new List<CustomFormat>
                        {
                            new CustomFormat("Hardcoded Subs", @"C_R_\b(?<hcsub>(\w+SUBS?)\b)|(?<hc>(HC|SUBBED))\b"),
                            new CustomFormat("Surround", @"C_R_\b((7|5).1)\b")
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
