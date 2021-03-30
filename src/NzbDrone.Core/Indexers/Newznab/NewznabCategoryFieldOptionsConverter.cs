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
            // Categories not relevant for Radarr
            var ignoreCategories = new[] { 1000, 3000, 4000, 6000, 7000 };

            // And maybe relevant for specific users
            var unimportantCategories = new[] { 0, 5000 };

            var result = new List<FieldSelectOption>();

            if (categories == null)
            {
                // Fetching categories failed, use default Newznab categories
                categories = new List<NewznabCategory>();
                categories.Add(new NewznabCategory
                {
                    Id = 2000,
                    Name = "Movies",
                    Subcategories = new List<NewznabCategory>
                    {
                        new NewznabCategory { Id = 2010, Name = "Foreign" },
                        new NewznabCategory { Id = 2020, Name = "Other" },
                        new NewznabCategory { Id = 2030, Name = "SD" },
                        new NewznabCategory { Id = 2040, Name = "HD" },
                        new NewznabCategory { Id = 2045, Name = "UHD" },
                        new NewznabCategory { Id = 2050, Name = "BluRay" },
                        new NewznabCategory { Id = 2060, Name = "3D" }
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
