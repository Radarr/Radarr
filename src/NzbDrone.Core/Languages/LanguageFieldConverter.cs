using System.Collections.Generic;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.Languages
{
    public class LanguageFieldConverter : ISelectOptionsConverter
    {
        public List<SelectOption> GetSelectOptions()
        {
            return Language.All.ConvertAll(v => new SelectOption { Value = v.Id, Name = v.Name });
        }
    }
}
