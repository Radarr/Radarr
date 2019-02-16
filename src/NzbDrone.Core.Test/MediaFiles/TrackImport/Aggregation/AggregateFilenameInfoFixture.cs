using NUnit.Framework;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Parser.Model;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using NzbDrone.Test.Common;
using NzbDrone.Core.MediaFiles.TrackImport.Aggregation.Aggregators;
using FluentAssertions;
using System.Text;
using System;
using System.Collections;

namespace NzbDrone.Core.Test.MediaFiles.TrackImport.Aggregation.Aggregators
{
    [TestFixture]
    public class AggregateFilenameInfoFixture : CoreTest<AggregateFilenameInfo>
    {

        private LocalAlbumRelease GivenTracks(List<string> files, string root)
        {
            var tracks = files.Select(x => new LocalTrack {
                    Path = Path.Combine(root, x),
                    FileTrackInfo = new ParsedTrackInfo {
                        TrackNumbers = new [] { 0 },
                    }
                }).ToList();
            return new LocalAlbumRelease(tracks);
        }

        private void VerifyData(LocalTrack track, string artist, string title, int trackNum, int disc)
        {
            track.FileTrackInfo.ArtistTitle.Should().Be(artist);
            track.FileTrackInfo.Title.Should().Be(title);
            track.FileTrackInfo.TrackNumbers[0].Should().Be(trackNum);
            track.FileTrackInfo.DiscNumber.Should().Be(disc);
        }

        [Test]
        public void should_aggregate_filenames_example()
        {
            var release = GivenTracks(new List<string> {
                    "Adele - 19 - 101 - Daydreamer.mp3",
                    "Adele - 19 - 102 - Best for Last.mp3",
                    "Adele - 19 - 103 - Chasing Pavements.mp3",
                    "Adele - 19 - 203 - That's It, I Quit, I'm Moving On.mp3"
                }, @"C:\incoming".AsOsAgnostic());

            Subject.Aggregate(release, true);

            VerifyData(release.LocalTracks[0], "Adele", "Daydreamer", 1, 1);
            VerifyData(release.LocalTracks[1], "Adele", "Best for Last", 2, 1);
            VerifyData(release.LocalTracks[2], "Adele", "Chasing Pavements", 3, 1);
            VerifyData(release.LocalTracks[3], "Adele", "That's It, I Quit, I'm Moving On", 3, 2);
        }

        public static class TestCaseFactory
        {
            private static List<string[]> tokenList = new List<string[]> {

                new [] {"trackNum2", "artist", "title", "tag"},
                new [] {"trackNum3", "artist", "title", "tag"},
                new [] {"trackNum2", "artist", "tag", "title"},
                new [] {"trackNum3", "artist", "tag", "title"},
                new [] {"trackNum2", "artist", "title"},
                new [] {"trackNum3", "artist", "title"},

                new [] {"artist", "tag", "trackNum2", "title"},
                new [] {"artist", "tag", "trackNum3", "title"},
                new [] {"artist", "trackNum2", "title", "tag"},
                new [] {"artist", "trackNum3", "title", "tag"},
                new [] {"artist", "trackNum2", "title"},
                new [] {"artist", "trackNum3", "title"},

                new [] {"artist", "title", "tag"},
                new [] {"artist", "tag", "title"},
                new [] {"artist", "title"},

                new [] {"trackNum2", "title"},
                new [] {"trackNum3", "title"},
                
                new [] {"title"},
            };

            private static List<Tuple<string, string>> separators = new List<Tuple<string, string>> {
                Tuple.Create(" - ", " "),
                Tuple.Create("_", " "),
                Tuple.Create("-", "_")
            };

            private static List<Tuple<string[], string, string>> otherCases = new List<Tuple<string[], string, string>> {
                Tuple.Create(new [] {"track2", "title"}, " ", " "),
                Tuple.Create(new [] {"track3", "title"}, " ", " ")
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
                    switch(field)
                    {
                        case "artist":
                            components.Add("artist name".Replace(" ", whitespace));
                            break;
                        case "tag":
                            components.Add("tag string ignore".Replace(" ", whitespace));
                            break;
                        case "title":
                            components.Add($"{(char)(96+i)} track title {i}".Replace(" ", whitespace));
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

        private void VerifyDataAuto(List<LocalTrack> tracks, string[] tokens, string whitespace)
        {
            for (int i = 1; i <= tracks.Count; i++)
            {
                var info = tracks[i-1].FileTrackInfo;

                if (tokens.Contains("artist"))
                {
                    info.ArtistTitle.Should().Be("artist name".Replace(" ", whitespace));
                }

                if (tokens.Contains("title"))
                {
                    info.Title.Should().Be($"{(char)(96+i)} track title {i}".Replace(" ", whitespace));
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
        
        [Test, TestCaseSource(typeof(TestCaseFactory), "TestCases")]
        public void should_aggregate_filenames_auto(Tuple<string[], string, string> testcase)
        {
            var files = GivenFilenames(testcase.Item1, testcase.Item2, testcase.Item3);
            var release = GivenTracks(files, @"C:\incoming".AsOsAgnostic());

            Subject.Aggregate(release, true);

            VerifyDataAuto(release.LocalTracks, testcase.Item1, testcase.Item3);
        }

    }
}
