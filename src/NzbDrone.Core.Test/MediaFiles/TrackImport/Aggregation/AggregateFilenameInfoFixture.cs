using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles.BookImport.Aggregation.Aggregators;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.BookImport.Aggregation.Aggregators
{
    [TestFixture]
    public class AggregateFilenameInfoFixture : CoreTest<AggregateFilenameInfo>
    {
        private LocalEdition GivenTracks(List<string> files, string root)
        {
            var tracks = files.Select(x => new LocalBook
            {
                Path = Path.Combine(root, x),
                FileTrackInfo = new ParsedTrackInfo
                {
                    TrackNumbers = new[] { 0 },
                }
            }).ToList();
            return new LocalEdition(tracks);
        }

        private void VerifyData(LocalBook track, string author, string title, int trackNum, int disc)
        {
            track.FileTrackInfo.AuthorTitle.Should().Be(author);
            track.FileTrackInfo.Title.Should().Be(title);
            track.FileTrackInfo.TrackNumbers[0].Should().Be(trackNum);
            track.FileTrackInfo.DiscNumber.Should().Be(disc);
        }

        [Test]
        public void should_aggregate_filenames_example()
        {
            var release = GivenTracks(new List<string>
            {
                    "Adele - 19 - 101 - Daydreamer.mp3",
                    "Adele - 19 - 102 - Best for Last.mp3",
                    "Adele - 19 - 103 - Chasing Pavements.mp3",
                    "Adele - 19 - 203 - That's It, I Quit, I'm Moving On.mp3"
            }, @"C:\incoming".AsOsAgnostic());

            Subject.Aggregate(release, true);

            VerifyData(release.LocalBooks[0], "Adele", "Daydreamer", 1, 1);
            VerifyData(release.LocalBooks[1], "Adele", "Best for Last", 2, 1);
            VerifyData(release.LocalBooks[2], "Adele", "Chasing Pavements", 3, 1);
            VerifyData(release.LocalBooks[3], "Adele", "That's It, I Quit, I'm Moving On", 3, 2);
        }

        public static class TestCaseFactory
        {
            private static List<string[]> tokenList = new List<string[]>
            {
                new[] { "trackNum2", "author", "title", "tag" },
                new[] { "trackNum3", "author", "title", "tag" },
                new[] { "trackNum2", "author", "tag", "title" },
                new[] { "trackNum3", "author", "tag", "title" },
                new[] { "trackNum2", "author", "title" },
                new[] { "trackNum3", "author", "title" },

                new[] { "author", "tag", "trackNum2", "title" },
                new[] { "author", "tag", "trackNum3", "title" },
                new[] { "author", "trackNum2", "title", "tag" },
                new[] { "author", "trackNum3", "title", "tag" },
                new[] { "author", "trackNum2", "title" },
                new[] { "author", "trackNum3", "title" },

                new[] { "author", "title", "tag" },
                new[] { "author", "tag", "title" },
                new[] { "author", "title" },

                new[] { "trackNum2", "title" },
                new[] { "trackNum3", "title" },

                new[] { "title" },
            };

            private static List<Tuple<string, string>> separators = new List<Tuple<string, string>>
            {
                Tuple.Create(" - ", " "),
                Tuple.Create("_", " "),
                Tuple.Create("-", "_")
            };

            private static List<Tuple<string[], string, string>> otherCases = new List<Tuple<string[], string, string>>
            {
                Tuple.Create(new[] { "track2", "title" }, " ", " "),
                Tuple.Create(new[] { "track3", "title" }, " ", " ")
            };

            public static IEnumerable TestCases
            {
                get
                {
                    int i = 0;

                    foreach (var tokens in tokenList)
                    {
                        foreach (var separator in separators)
                        {
                            i++;
                            yield return new TestCaseData(Tuple.Create(tokens, separator.Item1, separator.Item2))
                                .SetName($"should_aggregate_filenames_auto_{i}")
                                .SetDescription($"tokens: {string.Join(", ", tokens)}, separator: '{separator.Item1}', whitespace: '{separator.Item2}'");
                        }
                    }

                    // and a few other cases where all the permutations don't make sense
                    foreach (var item in otherCases)
                    {
                        i++;
                        yield return new TestCaseData(item)
                            .SetName($"should_aggregate_filenames_auto_{i}")
                            .SetDescription($"tokens: {string.Join(", ", item.Item1)}, separator: '{item.Item2}', whitespace: '{item.Item3}'");
                    }
                }
            }
        }

        private List<string> GivenFilenames(string[] fields, string fieldSeparator, string whitespace)
        {
            var outp = new List<string>();
            for (int i = 1; i <= 3; i++)
            {
                var components = new List<string>();
                foreach (var field in fields)
                {
                    switch (field)
                    {
                        case "author":
                            components.Add("author name".Replace(" ", whitespace));
                            break;
                        case "tag":
                            components.Add("tag string ignore".Replace(" ", whitespace));
                            break;
                        case "title":
                            components.Add($"{(char)(96 + i)} track title {i}".Replace(" ", whitespace));
                            break;
                        case "trackNum2":
                            components.Add(i.ToString("00"));
                            break;
                        case "trackNum3":
                            components.Add((100 + i).ToString("000"));
                            break;
                    }
                }

                outp.Add(string.Join(fieldSeparator, components) + ".mp3");
            }

            return outp;
        }

        private void VerifyDataAuto(List<LocalBook> tracks, string[] tokens, string whitespace)
        {
            for (int i = 1; i <= tracks.Count; i++)
            {
                var info = tracks[i - 1].FileTrackInfo;

                if (tokens.Contains("author"))
                {
                    info.AuthorTitle.Should().Be("author name".Replace(" ", whitespace));
                }

                if (tokens.Contains("title"))
                {
                    info.Title.Should().Be($"{(char)(96 + i)} track title {i}".Replace(" ", whitespace));
                }

                if (tokens.Contains("trackNum2") || tokens.Contains("trackNum3"))
                {
                    info.TrackNumbers[0].Should().Be(i);
                }

                if (tokens.Contains("trackNum3"))
                {
                    info.DiscNumber.Should().Be(1);
                }
                else
                {
                    info.DiscNumber.Should().Be(0);
                }
            }
        }

        [Test]
        [TestCaseSource(typeof(TestCaseFactory), "TestCases")]
        public void should_aggregate_filenames_auto(Tuple<string[], string, string> testcase)
        {
            var files = GivenFilenames(testcase.Item1, testcase.Item2, testcase.Item3);
            var release = GivenTracks(files, @"C:\incoming".AsOsAgnostic());

            Subject.Aggregate(release, true);

            VerifyDataAuto(release.LocalBooks, testcase.Item1, testcase.Item3);
        }
    }
}
