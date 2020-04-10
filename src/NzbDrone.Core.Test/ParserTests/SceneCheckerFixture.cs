using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class SceneCheckerFixture
    {
        //[TestCase("South.Park.S04E13.Helen.Keller.The.Musical.720p.WEBRip.AAC2.0.H.264-GC")]
        //[TestCase("Robot.Chicken.S07E02.720p.WEB-DL.DD5.1.H.264-pcsyndicate")]
        //[TestCase("Archer.2009.720p.WEB-DL.DD5.1.H.264-iT00NZ")]
        //[TestCase("30.Rock.S04E17.720p.HDTV.X264-DIMENSION")]
        //[TestCase("30.Rock.S04.720p.HDTV.X264-DIMENSION")]
        [TestCase("Kick.Ass.2.2013.German.DTS.DL.720p.BluRay.x264-Pate")]
        public void should_return_true_for_scene_names(string title)
        {
            SceneChecker.IsSceneTitle(title).Should().BeTrue();
        }

        [TestCase("S08E05 - Virtual In-Stanity [WEBDL-720p]")]
        [TestCase("S08E05 - Virtual In-Stanity.With.Dots [WEBDL-720p]")]
        [TestCase("Something")]
        [TestCase("86de66b7ef385e2fa56a3e41b98481ea1658bfab")]
        [TestCase("Kick.Ass.2.2013.German.DTS.DL.720p.BluRay.x264-", Description = "no group")]
        [TestCase("Kick.Ass.2.2013.German.DTS.DL-Pate", Description = "no quality")]
        [TestCase("2013.German.DTS.DL.BluRay.x264-Pate", Description = "no movietitle")]
        public void should_return_false_for_non_scene_names(string title)
        {
            SceneChecker.IsSceneTitle(title).Should().BeFalse();
        }

        [TestCase("Kick.Ass.2.2013.German.DTS.DL.720p.BluRay.x264-Pate_", "Kick.Ass.2.2013.German.DTS.DL.720p.BluRay.x264-Pate", Description = "underscore at the end")]
        [TestCase("Kick.Ass.2.2013.German.DTS.DL.720p.BluRay.x264-Pate.mkv", "Kick.Ass.2.2013.German.DTS.DL.720p.BluRay.x264-Pate", Description = "file extension")]
        [TestCase("Kick.Ass.2.2013.German.DTS.DL.720p.BluRay.x264-Pate.nzb", "Kick.Ass.2.2013.German.DTS.DL.720p.BluRay.x264-Pate", Description = "file extension")]
        [TestCase("Kick.Ass.2.2013.German.DTS.DL.【720p】.BluRay.x264-Pate.nzb", "Kick.Ass.2.2013.German.DTS.DL.[720p].BluRay.x264-Pate", Description = "brackets")]
        public void should_correctly_parse_scene_names(string title, string result)
        {
            SceneChecker.GetSceneTitle(title).Should().Be(result);
        }
    }
}
