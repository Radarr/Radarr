using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Annotations;

namespace NzbDrone.Core.Indexers.Newznab
{
    public static class NewznabCategoryFieldOptionsConverter
    {
        public static List<FieldSelectOption> GetFieldSelectOptions(List<NewznabCategory> categories)
        {
            // Ignore categories not relevant for Lidarr
            var ignoreCategories = new[] { 0, 1000, 2000, 4000, 5000, 6000, 7000 };

            var result = new List<FieldSelectOption>();

            if (categories == null)
            {
                // Fetching categories failed, use default Newznab categories
                categories = new List<NewznabCategory>();
                categories.Add(new NewznabCategory
                {
                    Id = 3000,
                    Name = "Music",
                    Subcategories = new List<NewznabCategory>
                    {
                        new NewznabCategory { Id = 3040, Name = "Loseless" },
                        new NewznabCategory { Id = 3010, Name = "MP3" },
                        new NewznabCategory { Id = 3050, Name = "Other" },
                        new NewznabCategory { Id = 3030, Name = "Audiobook" }
                    }
                });
            }

            foreach (var category in categories)
            {
                if (ignoreCategories.Contains(category.Id))
                {
                    continue;
                }

                result.Add(new FieldSelectOption
                {
                    Value = category.Id,
                    Name = category.Name,
                    Hint = $"({category.Id})"
                });

                if (category.Subcategories != null)
                {
                    foreach (var subcat in category.Subcategories)
                    {
                        result.Add(new FieldSelectOption
                        {
                            Value = subcat.Id,
                            Name = subcat.Name,
                            Hint = $"({subcat.Id})",
                            ParentValue = category.Id
                        });
                    }
                }
            }

            result.Sort((l, r) => l.Value.CompareTo(r.Value));

            return result;
        }
    }
}
