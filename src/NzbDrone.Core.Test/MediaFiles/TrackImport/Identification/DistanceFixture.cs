using NUnit.Framework;
using NzbDrone.Core.MediaFiles.TrackImport.Identification;
using NzbDrone.Test.Common;
using FluentAssertions;
using System.Collections.Generic;

namespace NzbDrone.Core.Test.MediaFiles.TrackImport.Identification
{
    [TestFixture]
    public class DistanceFixture : TestBase
    {
        [Test]
        public void test_add()
        {
            var dist = new Distance();
            dist.Add("add", 1.0);
            dist.Penalties.ShouldBeEquivalentTo(new Dictionary<string, List<double>> { {"add", new List<double> { 1.0 }}} );
        }

        [Test]
        public void test_equality()
        {
            var dist = new Distance();
            dist.AddEquality("equality", "ghi", new List<string> { "abc", "def", "ghi" });
            dist.Penalties.ShouldBeEquivalentTo(new Dictionary<string, List<double>> { {"equality", new List<double> { 0.0 }}} );

            dist.AddEquality("equality", "xyz", new List<string> { "abc", "def", "ghi" });
            dist.Penalties.ShouldBeEquivalentTo(new Dictionary<string, List<double>> { {"equality", new List<double> { 0.0, 1.0 }}} );

            dist.AddEquality("equality", "abc", new List<string> { "abc", "def", "ghi" });
            dist.Penalties.ShouldBeEquivalentTo(new Dictionary<string, List<double>> { {"equality", new List<double> { 0.0, 1.0, 0.0 }}} );
        }

        [Test]
        public void test_add_bool()
        {
            var dist = new Distance();
            dist.AddBool("expr", true);
            dist.Penalties.ShouldBeEquivalentTo(new Dictionary<string, List<double>> { {"expr", new List<double> { 1.0 }}} );

            dist.AddBool("expr", false);
            dist.Penalties.ShouldBeEquivalentTo(new Dictionary<string, List<double>> { {"expr", new List<double> { 1.0, 0.0 }}} );
        }

        [Test]
        public void test_add_number()
        {
            var dist = new Distance();
            dist.AddNumber("number", 1, 1);
            dist.Penalties.ShouldBeEquivalentTo(new Dictionary<string, List<double>> { {"number", new List<double> { 0.0 }}} );

            dist.AddNumber("number", 1, 2);
            dist.Penalties.ShouldBeEquivalentTo(new Dictionary<string, List<double>> { {"number", new List<double> { 0.0, 1.0 }}} );

            dist.AddNumber("number", 2, 1);
            dist.Penalties.ShouldBeEquivalentTo(new Dictionary<string, List<double>> { {"number", new List<double> { 0.0, 1.0, 1.0 }}} );

            dist.AddNumber("number", -1, 2);
            dist.Penalties.ShouldBeEquivalentTo(new Dictionary<string, List<double>> { {"number", new List<double> { 0.0, 1.0, 1.0, 1.0, 1.0, 1.0 }}} );
        }

        [Test]
        public void test_add_priority_value()
        {
            var dist = new Distance();
            dist.AddPriority("priority", "abc", new List<string> { "abc" });
            dist.Penalties.ShouldBeEquivalentTo(new Dictionary<string, List<double>> { {"priority", new List<double> { 0.0 }}} );

            dist.AddPriority("priority", "def", new List<string> { "abc", "def" });
            dist.Penalties.ShouldBeEquivalentTo(new Dictionary<string, List<double>> { {"priority", new List<double> { 0.0, 0.5 }}} );

            dist.AddPriority("priority", "xyz", new List<string> { "abc", "def" });
            dist.Penalties.ShouldBeEquivalentTo(new Dictionary<string, List<double>> { {"priority", new List<double> { 0.0, 0.5, 1.0 }}} );
        }

        [Test]
        public void test_add_priority_list()
        {
            var dist = new Distance();
            dist.AddPriority("priority", new List<string> { "abc" }, new List<string> { "abc" });
            dist.Penalties.ShouldBeEquivalentTo(new Dictionary<string, List<double>> { {"priority", new List<double> { 0.0 }}} );

            dist.AddPriority("priority", new List<string> { "def" }, new List<string> { "abc" });
            dist.Penalties.ShouldBeEquivalentTo(new Dictionary<string, List<double>> { {"priority", new List<double> { 0.0, 1.0 }}} );

            dist.AddPriority("priority", new List<string> { "abc", "xyz" }, new List<string> { "abc" });
            dist.Penalties.ShouldBeEquivalentTo(new Dictionary<string, List<double>> { {"priority", new List<double> { 0.0, 1.0, 0.0 }}} );

            dist.AddPriority("priority", new List<string> { "def", "xyz" }, new List<string> { "abc", "def" });
            dist.Penalties.ShouldBeEquivalentTo(new Dictionary<string, List<double>> { {"priority", new List<double> { 0.0, 1.0, 0.0, 0.5 }}} );
        }

        [Test]
        public void test_add_ratio()
        {
            var dist = new Distance();
            dist.AddRatio("ratio", 25, 100);
            dist.Penalties.ShouldBeEquivalentTo(new Dictionary<string, List<double>> { {"ratio", new List<double> { 0.25 }}} );

            dist.AddRatio("ratio", 10, 5);
            dist.Penalties.ShouldBeEquivalentTo(new Dictionary<string, List<double>> { {"ratio", new List<double> { 0.25, 1.0 }}} );

            dist.AddRatio("ratio", -5, 5);
            dist.Penalties.ShouldBeEquivalentTo(new Dictionary<string, List<double>> { {"ratio", new List<double> { 0.25, 1.0, 0.0 }}} );

            dist.AddRatio("ratio", 5, 0);
            dist.Penalties.ShouldBeEquivalentTo(new Dictionary<string, List<double>> { {"ratio", new List<double> { 0.25, 1.0, 0.0, 0.0 }}} );
        }

        [Test]
        public void test_add_string()
        {
            var dist = new Distance();
            dist.AddString("string", "abcd", "bcde");
            dist.Penalties.ShouldBeEquivalentTo(new Dictionary<string, List<double>> { {"string", new List<double> { 0.5 }}} );
        }

        [Test]
        public void test_add_string_none()
        {
            var dist = new Distance();
            dist.AddString("string", string.Empty, "bcd");
            dist.Penalties.ShouldBeEquivalentTo(new Dictionary<string, List<double>> { {"string", new List<double> { 1.0 }}} );
        }
        
        [Test]
        public void test_add_string_both_none()
        {
            var dist = new Distance();
            dist.AddString("string", string.Empty, string.Empty);
            dist.Penalties.ShouldBeEquivalentTo(new Dictionary<string, List<double>> { {"string", new List<double> { 0.0 }}} );
        }

        [Test]
        public void test_distance()
        {
            var dist = new Distance();
            dist.Add("album", 0.5);
            dist.Add("media_count", 0.25);
            dist.Add("media_count", 0.75);

            dist.NormalizedDistance().Should().Be(0.5);
        }

        [Test]
        public void test_max_distance()
        {
            var dist = new Distance();
            dist.Add("album", 0.5);
            dist.Add("media_count", 0.0);
            dist.Add("media_count", 0.0);

            dist.MaxDistance().Should().Be(5.0);
        }

        [Test]
        public void test_raw_distance()
        {
            var dist = new Distance();
            dist.Add("album", 0.5);
            dist.Add("media_count", 0.25);
            dist.Add("media_count", 0.5);

            dist.RawDistance().Should().Be(2.25);
        }
    }
}
