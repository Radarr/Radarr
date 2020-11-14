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
            // Ignore categories not relevant for Readarr
            var ignoreCategories = new[] { 1000, 2000, 3000, 4000, 5000, 6000 };

            // And maybe relevant for specific users
            var unimportantCategories = new[] { 0, 8000 };

            var result = new List<FieldSelectOption>();

            if (categories == null)
            {
                // Fetching categories failed, use default Newznab categories
                categories = new List<NewznabCategory>();
                categories.Add(new NewznabCategory
                {
                    Id = 7000,
                    Name = "Books",
                    Subcategories = new List<NewznabCategory>
                    {
                        new NewznabCategory { Id = 7010, Name = "Misc books" },
                        new NewznabCategory { Id = 7020, Name = "Ebook" },
                        new NewznabCategory { Id = 7030, Name = "Comics" },
                        new NewznabCategory { Id = 7040, Name = "Magazines" }
                    }
                });
            }

            foreach (var category in categories.Where(cat => !ignoreCategories.Contains(cat.Id)).OrderBy(cat => unimportantCategories.Contains(cat.Id)).ThenBy(cat => cat.Id))
            {
                result.Add(new FieldSelectOption
                {
                    Value = category.Id,
                    Name = category.Name,
                    Hint = $"({category.Id})"
                });

                if (category.Subcategories != null)
                {
                    foreach (var subcat in category.Subcategories.OrderBy(cat => cat.Id))
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

            return result;
        }
    }
}
