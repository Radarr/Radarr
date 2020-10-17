using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.Languages
{
    public class RealLanguageFieldConverter : ISelectOptionsConverter
    {
        public List<SelectOption> GetSelectOptions()
        {
            return Language.All
                .Where(l => l != Language.Unknown && l != Language.Any)
                .ToList()
                .ConvertAll(v => new SelectOption { Value = v.Id, Name = v.Name });
        }
    }
}
