using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Parser;
using Radarr.Http;

namespace Radarr.Api.V2.Profiles.Languages
{
    public class LanguageModule : RadarrRestModule<LanguageResource>
    {
        public LanguageModule()
        {
            GetResourceAll = GetAll;
            GetResourceById = GetById;
        }

        private LanguageResource GetById(int id)
        {
            var language = (Language)id;

            return new LanguageResource
            {
                Id = (int)language,
                Name = language.ToString()
            };
        }

        private List<LanguageResource> GetAll()
        {
            return ((Language[])Enum.GetValues(typeof (Language)))
                                    .Select(l => new LanguageResource
                                                    {
                                                        Id = (int) l,
                                                        Name = l.ToString()
                                                    })
                                    .OrderBy(l => l.Name)
                                    .ToList();
        }
    }
}
