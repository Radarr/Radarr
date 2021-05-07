using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class ReleaseGroupParserFixture : CoreTest
    {
        [TestCase("Movie.2009.S01E14.English.HDTV.XviD-LOL", "LOL")]
        [TestCase("Movie 2009 S01E14 English HDTV XviD LOL", null)]
        [TestCase("Acropolis Now S05 EXTRAS DVDRip XviD RUNNER", null)]
        [TestCase("Punky.Brewster.S01.EXTRAS.DVDRip.XviD-RUNNER", "RUNNER")]
        [TestCase("2020.NZ.2011.12.02.PDTV.XviD-C4TV", "C4TV")]
        [TestCase("Some.Movie.S03E115.DVDRip.XviD-OSiTV", "OSiTV")]
        [TestCase("Some Movie - S01E01 - Pilot [HTDV-480p]", null)]
        [TestCase("Some Movie - S01E01 - Pilot [HTDV-720p]", null)]
        [TestCase("Some Movie - S01E01 - Pilot [HTDV-1080p]", null)]
        [TestCase("Movie.Name.S04E13.720p.WEB-DL.AAC2.0.H.264-Cyphanix", "Cyphanix")]
        [TestCase("Movie.Name.S02E01.720p.WEB-DL.DD5.1.H.264.mkv", null)]
        [TestCase("Series Title S01E01 Episode Title", null)]
        [TestCase("Movie.Name- 2014-06-02 - Some Movie.mkv", null)]
        [TestCase("Movie.Name S12E17 May 23, 2014.mp4", null)]
        [TestCase("Movie.Name - S01E08 - Transistri\u00EB, Zuid-Osseti\u00EB en Abchazi\u00EB SDTV.avi", null)]
        [TestCase("Movie.Name 10x11 - Wild Movies Cant Be Broken [rl].avi", "rl")]
        [TestCase("[ www.Torrenting.com ] - Movie.Name.S03E14.720p.HDTV.X264-DIMENSION", "DIMENSION")]
        [TestCase("Movie.Name S02E09 HDTV x264-2HD [eztv]-[rarbg.com]", "2HD")]
        [TestCase("7s-Movie.Name-s02e01-720p.mkv", null)]
        [TestCase("The.Movie.Name.720p.HEVC.x265-MeGusta-Pre", "MeGusta")]
        [TestCase("Blue.Movie.Name.S08E05.The.Movie.1080p.AMZN.WEB-DL.DDP5.1.H.264-NTb-Rakuv", "NTb")]
        [TestCase("Movie.Name.S01E13.720p.BluRay.x264-SiNNERS-Rakuvfinhel", "SiNNERS")]
        [TestCase("Movie.Name.S01E01.INTERNAL.720p.HDTV.x264-aAF-RakuvUS-Obfuscated", "aAF")]
        [TestCase("Movie.Name.2018.720p.WEBRip.DDP5.1.x264-NTb-postbot", "NTb")]
        [TestCase("Movie.Name.2018.720p.WEBRip.DDP5.1.x264-NTb-xpost", "NTb")]
        [TestCase("Movie.Name.S02E24.1080p.AMZN.WEBRip.DD5.1.x264-CasStudio-AsRequested", "CasStudio")]
        [TestCase("Movie.Name.S04E11.Lamster.1080p.AMZN.WEB-DL.DDP5.1.H.264-NTb-AlternativeToRequested", "NTb")]
        [TestCase("Movie.Name.S16E04.Third.Wheel.1080p.AMZN.WEB-DL.DDP5.1.H.264-NTb-GEROV", "NTb")]
        [TestCase("Movie.NameS10E06.Kid.n.Play.1080p.AMZN.WEB-DL.DDP5.1.H.264-NTb-Z0iDS3N", "NTb")]
        [TestCase("Movie.Name.S02E06.The.House.of.Lords.DVDRip.x264-MaG-Chamele0n", "MaG")]
        [TestCase("Some.Movie.2013.1080p.BluRay.REMUX.AVC.DTS-X.MA.5.1", null)]
        [TestCase("Some.Movie.2013.1080p.BluRay.REMUX.AVC.DTS-MA.5.1", null)]
        [TestCase("Movie.Name.2013.1080p.BluRay.REMUX.AVC.DTS-ES.MA.5.1", null)]
        [TestCase("SomeMovie.1080p.BluRay.DTS-X.264.-D-Z0N3.mkv", "D-Z0N3")]
        [TestCase("SomeMovie.1080p.BluRay.DTS.x264.-Blu-bits.mkv", "Blu-bits")]
        [TestCase("SomeMovie.1080p.BluRay.DTS.x264.-DX-TV.mkv", "DX-TV")]
        [TestCase("SomeMovie.1080p.BluRay.DTS.x264.-FTW-HS.mkv", "FTW-HS")]
        [TestCase("SomeMovie.1080p.BluRay.DTS.x264.-VH-PROD.mkv", "VH-PROD")]
        [TestCase("Some.Dead.Movie.2006.1080p.BluRay.DTS.x264.D-Z0N3", "D-Z0N3")]
        [TestCase("Movie.Title.2010.720p.BluRay.x264.-[YTS.LT]", "YTS.LT")]

        //[TestCase("", "")]

        //Exception Cases
        public void should_parse_exception_release_group(string title, string expected)
        {
            Parser.Parser.ParseReleaseGroup(title).Should().Be(expected);
        }

        [TestCase("Movie Name (2020) [2160p x265 10bit S82 Joy]", "Joy")]
        [TestCase("Movie Name (2003) (2160p BluRay X265 HEVC 10bit HDR AAC 7.1 Tigole) [QxR]", "Tigole")]
        [TestCase("Ode To Joy (2009) (2160p BluRay x265 10bit HDR Joy)", "Joy")]
        [TestCase("Movie Name (2001) 1080p NF WEB-DL DDP2.0 x264-E.N.D", "E.N.D")]
        [TestCase("Movie Name (2020) [1080p] [WEBRip] [5.1] [YTS.MX]", "YTS.MX")]
        [TestCase("Movie Name.2018.1080p.Blu-ray.Remux.AVC.DTS-HD.MA.5.1.KRaLiMaRKo", "KRaLiMaRKo")]
        [TestCase("Ode To Joy (2009) (2160p BluRay x265 10bit HDR FreetheFish)", "FreetheFish")]
        [TestCase("Ode To Joy (2009) (2160p BluRay x265 10bit HDR afm72)", "afm72")]
        [TestCase("Ode To Joy (2009) (2160p BluRay x265 10bit HDR)", null)]
        [TestCase("Movie Name (2012) (1080p BluRay x265 HEVC 10bit AC3 2.0 Anna)", "Anna")]
        [TestCase("Movie Name (2019) (1080p BluRay x265 HEVC 10bit AAC 7.1 Q22 Joy)", "Joy")]
        [TestCase("Movie Name (2019) (2160p BluRay x265 HEVC 10bit HDR AAC 7.1 Bandi)", "Bandi")]
        [TestCase("Movie Name (2009) (1080p HDTV x265 HEVC 10bit AAC 2.0 Ghost)", "Ghost")]
        [TestCase("Movie Name in the Movie (2017) (1080p BluRay x265 HEVC 10bit AAC 7.1 Tigole)", "Tigole")]
        [TestCase("Mission - Movie Name - Movie Protocol (2011) (1080p BluRay x265 HEVC 10bit AAC 7.1 Tigole)", "Tigole")]
        [TestCase("Movie Name (1990) (1080p BluRay x265 HEVC 10bit AAC 5.1 Silence)", "Silence")]
        [TestCase("Happy Movie Name (1999) (1080p BluRay x265 HEVC 10bit AAC 5.1 Korean Kappa)", "Kappa")]
        [TestCase("Movie Name (2007) Open Matte (1080p AMZN WEB-DL x265 HEVC 10bit AAC 5.1 MONOLITH)", "MONOLITH")]
        [TestCase("Movie-Name (2019) (1080p BluRay x265 HEVC 10bit DTS 7.1 Qman)", "Qman")]
        [TestCase("Movie Name - Hell to Ticket (2018) + Extras (1080p BluRay x265 HEVC 10bit AAC 5.1 RZeroX)", "RZeroX")]
        [TestCase("Movie Name (2013) (Diamond Luxe Edition) + Extras (1080p BluRay x265 HEVC 10bit EAC3 7.1 SAMPA)", "SAMPA")]
        [TestCase("Movie Name 1984 (2020) (1080p AMZN WEB-DL x265 HEVC 10bit EAC3 5.1 Silence)", "Silence")]
        [TestCase("The.Movie.of.the.Name.1991.REMASTERED.720p.10bit.BluRay.6CH.x265.HEVC-PSA", "PSA")]
        [TestCase("Movie Name 2016 (1080p BluRay x265 HEVC 10bit DDP 5.1 theincognito)", "theincognito")]
        [TestCase("Movie Name - A History of Movie (2017) (1080p AMZN WEB-DL x265 HEVC 10bit EAC3 2.0 t3nzin)", "t3nzin")]
        [TestCase("Movie Name (2019) (1080p BluRay x265 HEVC 10bit AAC 7.1 Vyndros)", "Vyndros")]

        public void should_parse_release_group(string title, string expected)
        {
            Parser.Parser.ParseReleaseGroup(title).Should().Be(expected);
        }

        [TestCase("Some.Movie.S02E04.720p.WEBRip.x264-SKGTV English", "SKGTV")]
        [TestCase("Some.Movie.S02E04.720p.WEBRip.x264-SKGTV_English", "SKGTV")]
        [TestCase("Some.Movie.S02E04.720p.WEBRip.x264-SKGTV.English", "SKGTV")]

        //[TestCase("", "")]
        public void should_not_include_language_in_release_group(string title, string expected)
        {
            Parser.Parser.ParseReleaseGroup(title).Should().Be(expected);
        }

        [TestCase("Some.Movie.2019.1080p.BDRip.X264.AC3-EVO-RP", "EVO")]
        [TestCase("Some.Movie.2019.1080p.BDRip.X264.AC3-EVO-RP-RP", "EVO")]
        [TestCase("Some.Movie.2019.1080p.BDRip.X264.AC3-EVO-Obfuscation", "EVO")]
        [TestCase("Some.Movie.2019.1080p.BDRip.X264.AC3-EVO-NZBgeek", "EVO")]
        [TestCase("Some.Movie.2019.1080p.BDRip.X264.AC3-EVO-1", "EVO")]
        [TestCase("Some.Movie.2019.1080p.BDRip.X264.AC3-EVO-sample.mkv", "EVO")]
        [TestCase("Some.Movie.2019.1080p.BDRip.X264.AC3-EVO-Scrambled", "EVO")]
        [TestCase("Some.Movie.2019.1080p.BDRip.X264.AC3-EVO-postbot", "EVO")]
        [TestCase("Some.Movie.2019.1080p.BDRip.X264.AC3-EVO-xpost", "EVO")]
        [TestCase("Some.Movie.2019.1080p.BDRip.X264.AC3-EVO-Rakuv", "EVO")]
        [TestCase("Some.Movie.2019.1080p.BDRip.X264.AC3-EVO-Rakuv02", "EVO")]
        [TestCase("Some.Movie.2019.1080p.BDRip.X264.AC3-EVO-Rakuvfinhel", "EVO")]
        [TestCase("Some.Movie.2019.1080p.BDRip.X264.AC3-EVO-Obfuscated", "EVO")]
        [TestCase("Some.Movie.2019.1080p.BDRip.X264.AC3-EVO-WhiteRev", "EVO")]
        [TestCase("Some.Movie.2019.1080p.BDRip.X264.AC3-EVO-BUYMORE", "EVO")]
        [TestCase("Some.Movie.2019.1080p.BDRip.X264.AC3-EVO-AsRequested", "EVO")]
        [TestCase("Some.Movie.2019.1080p.BDRip.X264.AC3-EVO-AlternativeToRequested", "EVO")]
        [TestCase("Some.Movie.2019.1080p.BDRip.X264.AC3-EVO-GEROV", "EVO")]
        [TestCase("Some.Movie.2019.1080p.BDRip.X264.AC3-EVO-Z0iDS3N", "EVO")]
        [TestCase("Some.Movie.2019.1080p.BDRip.X264.AC3-EVO-Chamele0n", "EVO")]
        [TestCase("Some.Movie.2019.1080p.BDRip.X264.AC3-EVO-4P", "EVO")]
        [TestCase("Some.Movie.2019.1080p.BDRip.X264.AC3-EVO-4Planet", "EVO")]
        [TestCase("Some.Movie.2019.1080p.BDRip.X264.AC3-DON-AlteZachen", "DON")]
        public void should_not_include_repost_in_release_group(string title, string expected)
        {
            Parser.Parser.ParseReleaseGroup(title).Should().Be(expected);
        }

        [TestCase("[FFF] Invaders of the Movies!! - S01E11 - Someday, With Movies", "FFF")]
        [TestCase("[HorribleSubs] Invaders of the Movies!! - S01E12 - Movies Going Well!!", "HorribleSubs")]
        [TestCase("[Anime-Koi] Movies - S01E06 - Guys From Movies", "Anime-Koi")]
        [TestCase("[Anime-Koi] Movies - S01E07 - A High-Grade Movies", "Anime-Koi")]
        [TestCase("[Anime-Koi] Kami-sama Movies 2 - 01 [h264-720p][28D54E2C]", "Anime-Koi")]

        //[TestCase("", "")]
        public void should_parse_anime_release_groups(string title, string expected)
        {
            Parser.Parser.ParseReleaseGroup(title).Should().Be(expected);
        }
    }
}
