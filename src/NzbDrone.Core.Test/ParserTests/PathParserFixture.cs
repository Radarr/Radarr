namespace NzbDrone.Core.Test.ParserTests
{
    /*
        [TestFixture]
        [Ignore("Series")]//Is this really necessary with movies? I dont think so
        public class PathParserFixture : CoreTest
        {
            [TestCase(@"z:\tv shows\battlestar galactica (2003)\Season 3\S03E05 - Collaborators.mkv", 3, 5)]
            [TestCase(@"z:\tv shows\modern marvels\Season 16\S16E03 - The Potato.mkv", 16, 3)]
            [TestCase(@"z:\tv shows\robot chicken\Specials\S00E16 - Dear Consumer - SD TV.avi", 0, 16)]
            [TestCase(@"D:\shares\TV Shows\Parks And Recreation\Season 2\S02E21 - 94 Meetings - 720p TV.mkv", 2, 21)]
            [TestCase(@"D:\shares\TV Shows\Battlestar Galactica (2003)\Season 2\S02E21.avi", 2, 21)]
            [TestCase("C:/Test/TV/Chuck.4x05.HDTV.XviD-LOL", 4, 5)]
            [TestCase(@"P:\TV Shows\House\Season 6\S06E13 - 5 to 9 - 720p BluRay.mkv", 6, 13)]
            [TestCase(@"S:\TV Drop\House - 10x11 - Title [SDTV]\1011 - Title.avi", 10, 11)]
            [TestCase(@"/TV Drop/House - 10x11 - Title [SDTV]/1011 - Title.avi", 10, 11)]
            [TestCase(@"S:\TV Drop\King of the Hill - 10x12 - 24 Hour Propane People [SDTV]\1012 - 24 Hour Propane People.avi", 10, 12)]
            [TestCase(@"/TV Drop/King of the Hill - 10x12 - 24 Hour Propane People [SDTV]/1012 - 24 Hour Propane People.avi", 10, 12)]
            [TestCase(@"S:\TV Drop\King of the Hill - 10x12 - 24 Hour Propane People [SDTV]\Hour Propane People.avi", 10, 12)]
            [TestCase(@"/TV Drop/King of the Hill - 10x12 - 24 Hour Propane People [SDTV]/Hour Propane People.avi", 10, 12)]
            [TestCase(@"E:\Downloads\tv\The.Big.Bang.Theory.S01E01.720p.HDTV\ajifajjjeaeaeqwer_eppj.avi", 1, 1)]
            [TestCase(@"C:\Test\Unsorted\The.Big.Bang.Theory.S01E01.720p.HDTV\tbbt101.avi", 1, 1)]
            [TestCase(@"C:\Test\Unsorted\Terminator.The.Sarah.Connor.Chronicles.S02E19.720p.BluRay.x264-SiNNERS-RP\ba27283b17c00d01193eacc02a8ba98eeb523a76.mkv", 2, 19)]
            [TestCase(@"C:\Test\Unsorted\Terminator.The.Sarah.Connor.Chronicles.S02E18.720p.BluRay.x264-SiNNERS-RP\45a55debe3856da318cc35882ad07e43cd32fd15.mkv", 2, 18)]
            [TestCase(@"C:\Test\Series\Season 01\01 Pilot (1080p HD).mkv", 1, 1)]
            [TestCase(@"C:\Test\Series\Season 01\1 Pilot (1080p HD).mkv", 1, 1)]
            [TestCase(@"C:\Test\Series\Season 1\02 Honor Thy Father (1080p HD).m4v", 1, 2)]
            [TestCase(@"C:\Test\Series\Season 1\2 Honor Thy Father (1080p HD).m4v", 1, 2)]
    //        [TestCase(@"C:\CSI.NY.S02E04.720p.WEB-DL.DD5.1.H.264\73696S02-04.mkv", 2, 4)] //Gets treated as S01E04 (because it gets parsed as anime)
            public void should_parse_from_path(string path, string title)
            {
                var result = Parser.Parser.ParseMoviePath(path.AsOsAgnostic(), false);
                result.MovieTitle.Should().Be(title);

                ExceptionVerification.IgnoreWarns();
            }
        }
        */
}
