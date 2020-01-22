using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Core.Profiles;

namespace NzbDrone.Core.CustomFormats
{
    public class CustomFormatsComparer : IComparer<List<CustomFormat>>
    {
        private readonly Profile _profile;

        public CustomFormatsComparer(Profile profile)
        {
            Ensure.That(profile, () => profile).IsNotNull();
            Ensure.That(profile.Items, () => profile.Items).HasItems();

            _profile = profile;
        }

        public int Compare(List<CustomFormat> left, List<CustomFormat> right)
        {
            var leftIndicies = _profile.GetIndices(left);
            var rightIndicies = _profile.GetIndices(right);

            // Summing powers of two ensures last format always trumps, but we order correctly if we
            // have extra formats lower down the list
            var leftTotal = leftIndicies.Select(x => Math.Pow(2, x)).Sum();
            var rightTotal = rightIndicies.Select(x => Math.Pow(2, x)).Sum();

            return leftTotal.CompareTo(rightTotal);
        }
    }
}
