using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Profiles;

namespace NzbDrone.Core.Qualities
{
    public class QualityModelComparer : IComparer<Quality>, IComparer<QualityModel>, IComparer<CustomFormat>, IComparer<List<CustomFormat>>
    {
        private readonly Profile _profile;

        public QualityModelComparer(Profile profile)
        {
            Ensure.That(profile, () => profile).IsNotNull();
            Ensure.That(profile.Items, () => profile.Items).HasItems();

            _profile = profile;
        }

        public int Compare(int left, int right, bool respectGroupOrder = false)
        {
            var leftIndex = _profile.GetIndex(left);
            var rightIndex = _profile.GetIndex(right);

            return leftIndex.CompareTo(rightIndex, respectGroupOrder);
        }

        public int Compare(Quality left, Quality right)
        {
            return Compare(left, right, false);
        }

        public int Compare(Quality left, Quality right, bool respectGroupOrder)
        {
            var leftIndex = _profile.GetIndex(left);
            var rightIndex = _profile.GetIndex(right);

            return leftIndex.CompareTo(rightIndex, respectGroupOrder);
        }

        public int Compare(QualityModel left, QualityModel right)
        {
            return Compare(left, right, false);
        }

        public int Compare(QualityModel left, QualityModel right, bool respectGroupOrder)
        {
            int result = Compare(left.Quality, right.Quality, respectGroupOrder);

            if (result == 0)
            {
                result = Compare(left.CustomFormats, right.CustomFormats);

                if (result == 0)
                {
                    result = left.Revision.CompareTo(right.Revision);
                }
            }

            return result;
        }

        public int Compare(List<CustomFormat> left, List<CustomFormat> right)
        {
            List<int> leftIndicies = GetIndicies(left, _profile);
            List<int> rightIndicies = GetIndicies(right, _profile);

            int leftTotal = leftIndicies.Sum();
            int rightTotal = rightIndicies.Sum();

            return leftTotal.CompareTo(rightTotal);
        }

        public static List<int> GetIndicies(List<CustomFormat> formats, Profile profile)
        {
            return formats.WithNone().Select(f => profile.FormatItems.FindIndex(v => Equals(v.Format, f))).ToList();
        }

        public int Compare(CustomFormat left, CustomFormat right)
        {
            int leftIndex = _profile.FormatItems.FindIndex(v => Equals(v.Format, left));
            int rightIndex = _profile.FormatItems.FindIndex(v => Equals(v.Format, right));

            return leftIndex.CompareTo(rightIndex);
        }

        public int Compare(List<CustomFormat> left, CustomFormat right)
        {
            left = left.WithNone();

            var leftIndicies = GetIndicies(left, _profile);
            var rightIndex = _profile.FormatItems.FindIndex(v => Equals(v.Format, right));

            return leftIndicies.Select(i => i.CompareTo(rightIndex)).Sum();
        }
    }
}
