using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]

    public class QualityParserFixture : CoreTest
    {
        [SetUp]
        public void Setup()
        {
            //QualityDefinitionServiceFixture.SetupDefaultDefinitions();
        }

        //public static object[] SelfQualityParserCases = QualityDefinition.DefaultQualityDefinitions.ToArray();
        public static object[] OtherSourceQualityParserCases =
        {
            new object[] { "SD TV", Source.TV, Resolution.R480p, Modifier.NONE },
            new object[] { "SD DVD",  Source.DVD, Resolution.R480p, Modifier.NONE },
            new object[] { "480p WEB-DL", Source.WEBDL, Resolution.R480p, Modifier.NONE },
            new object[] { "HD TV", Source.TV, Resolution.R720p, Modifier.NONE },
            new object[] { "1080p HD TV", Source.TV, Resolution.R1080p, Modifier.NONE },
            new object[] { "2160p HD TV", Source.TV, Resolution.R2160p, Modifier.NONE },
            new object[] { "720p WEB-DL", Source.WEBDL, Resolution.R720p, Modifier.NONE },
            new object[] { "1080p WEB-DL", Source.WEBDL, Resolution.R1080p, Modifier.NONE },
            new object[] { "2160p WEB-DL", Source.WEBDL, Resolution.R2160p, Modifier.NONE },
            new object[] { "720p BluRay", Source.BLURAY, Resolution.R720p, Modifier.NONE },
            new object[] { "1080p BluRay", Source.BLURAY, Resolution.R1080p, Modifier.NONE },
            new object[] { "2160p BluRay", Source.BLURAY, Resolution.R2160p, Modifier.NONE },
            new object[] { "1080p Remux", Source.BLURAY, Resolution.R1080p, Modifier.REMUX },
            new object[] { "2160p Remux", Source.BLURAY, Resolution.R2160p, Modifier.REMUX },
        };

        [TestCase("Movie.Title.3.2017.720p.TSRip.x264.AAC-Ozlem", false)]
        public void should_parse_ts(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Source.TELESYNC, proper, Resolution.R720p);
        }

        [TestCase("S07E23 .avi ", false)]
        [TestCase("Movie Name S02E01 HDTV XviD 2HD", false)]
        [TestCase("Movie Name S05E11 PROPER HDTV XviD 2HD", true)]
        [TestCase("Movie Name S02E08 HDTV x264 FTP", false)]
        [TestCase("Movie.Name.2011.S02E01.WS.PDTV.x264-TLA", false)]
        [TestCase("Movie Name.2011.S02E01.WS.PDTV.x264-REPACK-TLA", true)]
        [TestCase("Movie Name S01E04 DSR x264 2HD", false)]
        [TestCase("Movie Name S01E04 Mexicos Death Train DSR x264 MiNDTHEGAP", false)]
        [TestCase("Movie Name S11E03 has no periods or extension HDTV", false)]
        [TestCase("Movie Name.S04E05.HDTV.XviD-LOL", false)]
        [TestCase("Some.Movie.S02E15.avi", false)]
        [TestCase("Some.Movie.S02E15.xvid", false)]
        [TestCase("Some.Movie.S02E15.divx", false)]
        [TestCase("Some.Movie.S03E06.HDTV-WiDE", false)]
        [TestCase("Movie Name.S10E27.WS.DSR.XviD-2HD", false)]
        [TestCase("Movie Name.S03.TVRip.XviD-NOGRP", false)]
        public void should_parse_sdtv_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Source.TV, proper, Resolution.R480p);
        }

        [TestCase("Some.Movie.S03E06.DVDRip.XviD-WiDE", false)]
        [TestCase("Some.Movie.S03E06.DVD.Rip.XviD-WiDE", false)]
        [TestCase("the.Movie Name.1x13.circles.ws.xvidvd-tns", false)]
        [TestCase("the_movie.9x18.sunshine_days.ac3.ws_dvdrip_xvid-fov.avi", false)]
        [TestCase("The.Third.Movie Name.2008.DVDRip.360p.H264 iPod -20-40", false)]
        [TestCase("SomeMovie.2018.DVDRip.ts", false)]
        public void should_parse_dvd_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Source.DVD, proper, Resolution.R480p);
        }

        [TestCase("Some.Movie.Magic.Rainbow.2007.DVD5.NTSC", false)]
        [TestCase("Some.Movie.Magic.Rainbow.2007.DVD9.NTSC", false)]
        [TestCase("Some.Movie.Magic.Rainbow.2007.DVDR.NTSC", false)]
        [TestCase("Some.Movie.Magic.Rainbow.2007.DVD-R.NTSC", false)]
        public void should_parse_dvdr_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Source.DVD, proper, Resolution.R480p, Modifier.REMUX);
        }

        [TestCase("Movie.Name.S01E10.The.Leviathan.480p.WEB-DL.x264-mSD", false)]
        [TestCase("Movie.Name.S04E10.Glee.Actually.480p.WEB-DL.x264-mSD", false)]
        [TestCase("Movie.Name.S06E11.The.Santa.Simulation.480p.WEB-DL.x264-mSD", false)]
        [TestCase("Movie.Name.S02E04.480p.WEB.DL.nSD.x264-NhaNc3", false)]
        [TestCase("[HorribleSubs] Movie Title! 2018 [Web][MKV][h264][480p][AAC 2.0][Softsubs (HorribleSubs)]", false)]
        [TestCase("[SubsPlease] Movie Title (540p) [AB649D32].mkv", false)]
        [TestCase("[Erai-raws] Movie Title [540p][Multiple Subtitle].mkv", false)]
        public void should_parse_webdl480p_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Source.WEBDL, proper, Resolution.R480p);
        }

        [TestCase("Movie.Name.1x04.ITA.WEBMux.x264-NovaRip", false)]
        public void should_parse_webrip480p_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Source.WEBRIP, proper, Resolution.R480p);
        }

        [TestCase("Movie.Name (BD)(640x480(RAW) (BATCH 1) (1-13)", false)]
        [TestCase("Movie.Name.S01E05.480p.BluRay.DD5.1.x264-HiSD", false)]
        [TestCase("Movie.Name.S03E01-06.DUAL.BDRip.AC3.-HELLYWOOD", false)]
        [TestCase("Movie.Name.2011.LIMITED.BluRay.360p.H264-20-40", false)]
        public void should_parse_bluray480p_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Source.BLURAY, proper, Resolution.R480p);
        }

        [TestCase("Movie Name - S01E01 - Title [HDTV]", false)]
        [TestCase("Movie Name - S01E01 - Title [HDTV-720p]", false)]
        [TestCase("Movie.Name S04E87 REPACK 720p HDTV x264 aAF", true)]
        [TestCase("S07E23 - [HDTV-720p].mkv ", false)]
        [TestCase("Movie.Name - S22E03 - MoneyBART - HD TV.mkv", false)]
        [TestCase("Movie.Name.S08E05.720p.HDTV.X264-DIMENSION", false)]
        [TestCase(@"E:\Downloads\tv\Movie.Name.S01E01.720p.HDTV\ajifajjjeaeaeqwer_eppj.avi", false)]
        [TestCase("Movie.Name.S01E08.Tourmaline.Nepal.720p.HDTV.x264-DHD", false)]
        [TestCase("Movie.Name.US.S12E17.HR.WS.PDTV.X264-DIMENSION", false)]
        [TestCase("Movie.Name.The.Lost.Pilots.Movie.HR.WS.PDTV.x264-DHD", false)]
        [TestCase("Movie.Name.The.Lost.Pilots.Movie.HR.WS.PDTV.x264-DHD-Remux.mkv", false)]
        public void should_parse_hdtv720p_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Source.TV, proper, Resolution.R720p);
        }

        [TestCase("Movie Name.S07E01.ARE.YOU.1080P.HDTV.X264-QCF", false)]
        [TestCase("Movie Name.S07E01.ARE.YOU.1080P.HDTV.x264-QCF", false)]
        [TestCase("Movie Name.S07E01.ARE.YOU.1080P.HDTV.proper.X264-QCF", true)]
        [TestCase("Movie Name - S01E01 - Title [HDTV-1080p]", false)]
        [TestCase("Movie.Name.2020.1080i.HDTV.DD5.1.H.264-NOGRP", false)]
        public void should_parse_hdtv1080p_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Source.TV, proper, Resolution.R1080p);
        }

        [TestCase("Movie Name S01E04 Mexicos Death Train 720p WEB DL", false)]
        [TestCase("Movie Name S02E21 720p WEB DL DD5 1 H 264", false)]
        [TestCase("Movie Name S04E22 720p WEB DL DD5 1 H 264 NFHD", false)]
        [TestCase("Movie Name - S11E06 - D-Yikes! - 720p WEB-DL.mkv", false)]
        [TestCase("Some.Movie.S02E15.720p.WEB-DL.DD5.1.H.264-SURFER", false)]
        [TestCase("S07E23 - [WEBDL].mkv ", false)]
        [TestCase("Movie Name S04E22 720p WEB-DL DD5.1 H264-EbP.mkv", false)]
        [TestCase("Movie Name.S04.720p.Web-Dl.Dd5.1.h264-P2PACK", false)]
        [TestCase("Movie Name.S02E04.720p.WEB.DL.nSD.x264-NhaNc3", false)]
        [TestCase("Movie Name.S04E25.720p.iTunesHD.AVC-TVS", false)]
        [TestCase("Movie Name.S06E23.720p.WebHD.h264-euHD", false)]
        [TestCase("Movie Name.2016.03.14.720p.WEB.x264-spamTV", false)]
        [TestCase("Movie Name.2016.03.14.720p.WEB.h264-spamTV", false)]
        [TestCase("Movie Name.S01E01.The.Insanity.Principle.720p.WEB-DL.DD5.1.H.264-BD", false)]
        [TestCase("[HorribleSubs] Movie Title! 2018 [Web][MKV][h264][720p][AAC 2.0][Softsubs (HorribleSubs)]", false)]
        [TestCase("[HorribleSubs] Movie Title! 2018 [Web][MKV][h264][AAC 2.0][Softsubs (HorribleSubs)]", false)]
        [TestCase("Movie.Title.2013.960p.WEB-DL.AAC2.0.H.264-squalor", false)]
        public void should_parse_webdl720p_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Source.WEBDL, proper, Resolution.R720p);
        }

        [TestCase("Movie.Title.ITA.720p.WEBMux.x264-NovaRip", false)]
        [TestCase("Movie Name.S04E01.720p.WEBRip.AAC2.0.x264-NFRiP", false)]
        public void should_parse_webrip720p_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Source.WEBRIP, proper, Resolution.R720p);
        }

        [TestCase("Movie Name S09E03 1080p WEB DL DD5 1 H264 NFHD", false)]
        [TestCase("Movie Name S10E03 1080p WEB DL DD5 1 H 264 NFHD", false)]
        [TestCase("Movie.Name.S08E01.1080p.WEB-DL.DD5.1.H264-NFHD", false)]
        [TestCase("Movie.Name.S08E01.1080p.WEB-DL.proper.AAC2.0.H.264", true)]
        [TestCase("Movie Name S10E03 1080p WEB DL DD5 1 H 264 REPACK NFHD", true)]
        [TestCase("Movie.Name.S04E09.Swan.Song.1080p.WEB-DL.DD5.1.H.264-ECI", false)]
        [TestCase("Movie.Name.S06E11.The.Santa.Simulation.1080p.WEB-DL.DD5.1.H.264", false)]
        [TestCase("Movie.Name.Baby.S01E02.Night.2.[WEBDL-1080p].mkv", false)]
        [TestCase("Movie.Name.2016.03.14.1080p.WEB.x264-spamTV", false)]
        [TestCase("Movie.Name.2016.03.14.1080p.WEB.h264-spamTV", false)]
        [TestCase("Movie.Name.S01.1080p.WEB-DL.AAC2.0.AVC-TrollHD", false)]
        [TestCase("Series Title S06E08 1080p WEB h264-EXCLUSIVE", false)]
        [TestCase("Series Title S06E08 No One PROPER 1080p WEB DD5 1 H 264-EXCLUSIVE", true)]
        [TestCase("Series Title S06E08 No One PROPER 1080p WEB H 264-EXCLUSIVE", true)]
        [TestCase("The.Movie.Name.S25E21.Pay.Pal.1080p.WEB-DL.DD5.1.H.264-NTb", false)]
        [TestCase("The.Movie.Name.2017.1080p.WEB-DL.DD5.1.H.264.Remux.-NTb", false)]
        [TestCase("Movie.Name.2019.1080p.AMZN.WEB-DL.DDP5.1.H.264-NTG", false)]
        [TestCase("Movie.Name.2020.1080p.AMZN.WEB...", false)]
        [TestCase("Movie.Name.2020.1080p.AMZN.WEB.", false)]
        [TestCase("Movie Title - 2020 1080p Viva MKV WEB", false)]
        [TestCase("[HorribleSubs] Movie Title! 2018 [Web][MKV][h264][1080p][AAC 2.0][Softsubs (HorribleSubs)]", false)]
        [TestCase("Movie.Title.2020.MULTi.1080p.WEB.H264-ALLDAYiN (S:285/L:11)", false)]
        [TestCase("Movie Title (2020) MULTi WEB 1080p x264-JiHEFF (S:317/L:28)", false)]
        [TestCase("Movie.Titles.2020.1080p.NF.WEB.DD2.0.x264-SNEAkY", false)]
        public void should_parse_webdl1080p_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Source.WEBDL, proper, Resolution.R1080p);
        }

        [TestCase("Movie.Name.S04E01.iNTERNAL.1080p.WEBRip.x264-QRUS", false)]
        [TestCase("Movie.Name.1x04.ITA.1080p.WEBMux.x264-NovaRip", false)]
        [TestCase("Movie.Name.2019.S02E07.Chapter.15.The.Believer.4Kto1080p.DSNYP.Webrip.x265.10bit.EAC3.5.1.Atmos.GokiTAoE", false)]
        public void should_parse_webrip1080p_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Source.WEBRIP, proper, Resolution.R1080p);
        }

        [TestCase("Movie.Name.2016.03.14.2160p.WEB.x264-spamTV", false)]
        [TestCase("Movie.Name.2016.03.14.2160p.WEB.h264-spamTV", false)]
        [TestCase("Movie.Name.2016.03.14.2160p.WEB.PROPER.h264-spamTV", true)]
        [TestCase("[HorribleSubs] Movie Title! 2018 [Web][MKV][h264][2160p][AAC 2.0][Softsubs (HorribleSubs)]", false)]
        [TestCase("Movie Name 2020 WEB-DL 4K H265 10bit HDR DDP5.1 Atmos-PTerWEB", false)]
        public void should_parse_webdl2160p_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Source.WEBDL, proper, Resolution.R2160p);
        }

        [TestCase("Movie Name S01E01.2160P AMZN WEBRIP DD2.0 HI10P X264-TROLLUHD", false)]
        [TestCase("Movie ADD Name S01E01.2160P AMZN WEBRIP DD2.0 X264-TROLLUHD", false)]
        [TestCase("Movie.Name.S01E01.2160p.AMZN.WEBRip.DD2.0.Hi10p.X264-TrollUHD", false)]
        [TestCase("Movie Name S01E01 2160p AMZN WEBRip DD2.0 Hi10P x264-TrollUHD", false)]
        public void should_parse_webrip2160p_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Source.WEBRIP, proper, Resolution.R2160p);
        }

        [TestCase("Movie.Name.S03E01-06.DUAL.Bluray.AC3.-HELLYWOOD.avi", false)]
        [TestCase("Movie Name - S01E03 - Come Fly With Me - 720p BluRay.mkv", false)]
        [TestCase("Movie Name.S03E01.The Electric Can Opener Fluctuation.m2ts", false)]
        [TestCase("Movie.Name.S01E02.Chained.Heat.[Bluray720p].mkv", false)]
        [TestCase("[FFF] Movie Name - 01 [BD][720p-AAC][0601BED4]", false)]
        [TestCase("[coldhell] Movie v3 [BD720p][03192D4C]", false)]
        [TestCase("[RandomRemux] Movie - 01 [720p BD][043EA407].mkv", false)]
        [TestCase("[Kaylith] Movie Friends Movies - 01 [BD 720p AAC][B7EEE164].mkv", false)]
        [TestCase("Movie.Name.S03E01-06.DUAL.Blu-ray.AC3.-HELLYWOOD.avi", false)]
        [TestCase("Movie.Name.S03E01-06.DUAL.720p.Blu-ray.AC3.-HELLYWOOD.avi", false)]
        [TestCase("[Elysium]Movie.Name.01(BD.720p.AAC.DA)[0BB96AD8].mkv", false)]
        [TestCase("Movie.Name.S01E01.33.720p.HDDVD.x264-SiNNERS.mkv", false)]
        [TestCase("Movie.Name.S01E07.RERIP.720p.BluRay.x264-DEMAND", true)]
        [TestCase("Movie.Name.2016.2018.720p.MBluRay.x264-CRUELTY.mkv", false)]
        [TestCase("Movie.Name.2019.720p.MBLURAY.x264-MBLURAYFANS.mkv", false)]
        [TestCase("Movie.Name2017.720p.MBluRay.x264-TREBLE.mkv", false)]
        [TestCase("Movie.Name.2.Parte.2.ITA-ENG.720p.BDMux.DD5.1.x264-DarkSideMux", false)]
        public void should_parse_bluray720p_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Source.BLURAY, proper, Resolution.R720p);
        }

        [TestCase("Movie Title - S01E03 - Come Fly With Me - 1080p BluRay.mkv", false)]
        [TestCase("Movie.Title.S02E13.1080p.BluRay.x264-AVCDVD", false)]
        [TestCase("Movie.S01E02.Chained.Heat.[Bluray1080p].mkv", false)]
        [TestCase("[FFF] Movie no Muromi-san - 10 [BD][1080p-FLAC][0C4091AF]", false)]
        [TestCase("[coldhell] Movie v2 [BD1080p][5A45EABE].mkv", false)]
        [TestCase("[Kaylith] Movie Friends Specials - 01 [BD 1080p FLAC][429FD8C7].mkv", false)]
        [TestCase("[Zurako] Log Movie - 01 - The Movie (BD 1080p AAC) [7AE12174].mkv", false)]
        [TestCase("Movie.S03E01-06.DUAL.1080p.Blu-ray.AC3.-HELLYWOOD.avi", false)]
        [TestCase("[Coalgirls]_Movie!!_01_(1920x1080_Blu-ray_FLAC)_[8370CB8F].mkv", false)]
        [TestCase("Movie.Name.2016.2018.1080p.MBluRay.x264-CRUELTY.mkv", false)]
        [TestCase("Movie.Name.2019.1080p.MBLURAY.x264-MBLURAYFANS.mkv", false)]
        [TestCase("Movie.Name2017.1080p.MBluRay.x264-TREBLE.mkv", false)]
        [TestCase("Movie.Name.2011.UHD.BluRay.DD5.1.HDR.x265-CtrlHD/ctrlhd-rotpota-1080p.mkv", false)]
        [TestCase("Movie Name 2005 1080p UHD BluRay DD+7.1 x264-LoRD.mkv", false)]
        [TestCase("Movie.Name.2011.1080p.UHD.BluRay.DD5.1.HDR.x265-CtrlHD.mkv", false)]
        [TestCase("Movie.Name.2016.German.DTS.DL.1080p.UHDBD.x265-TDO.mkv", false)]
        public void should_parse_bluray1080p_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Source.BLURAY, proper, Resolution.R1080p);
        }

        [TestCase("Movie.S01E02.Chained.Heat.[Bluray2160p].mkv", false)]
        [TestCase("[FFF] Movie no Movie-san - 10 [BD][2160p-FLAC][0C4091AF]", false)]
        [TestCase("[coldhell] Movie v2 [BD2160p][5A45EABE].mkv", false)]
        [TestCase("[Kaylith] Movie Friends Specials - 01 [BD 2160p FLAC][429FD8C7].mkv", false)]
        [TestCase("[Zurako] Log Movie - 01 - The Movie (BD 2160p AAC) [7AE12174].mkv", false)]
        [TestCase("Movie.Title.S03E01-06.DUAL.2160p.Blu-ray.AC3.-HELLYWOOD.avi", false)]
        [TestCase("[Coalgirls]_Movie!!_01_(3840x2160_Blu-ray_FLAC)_[8370CB8F].mkv", false)]
        [TestCase("Movie.Title.2016.2018.2160p.MBluRay.x264-CRUELTY.mkv", false)]
        [TestCase("Movie.Title.2019.2160p.MBLURAY.x264-MBLURAYFANS.mkv", false)]
        [TestCase("Movie.Title.2017.2160p.MBluRay.x264-TREBLE.mkv", false)]
        [TestCase("Movie.Name.2020.German.UHDBD.2160p.HDR10.HEVC.EAC3.DL-pmHD.mkv", false)]
        [TestCase("Movie.Title.2014.2160p.UHD.BluRay.X265-IAMABLE.mkv", false)]
        [TestCase("Movie.Title.2014.2160p.BDRip.AAC.7.1.HDR10.x265.10bit-Markll", false)]
        public void should_parse_bluray2160p_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Source.BLURAY, proper, Resolution.R2160p);
        }

        [TestCase("Movie.Name.2004.576p.BDRip.x264-HANDJOB")]
        [TestCase("Movie.Title.S01E05.576p.BluRay.DD5.1.x264-HiSD")]
        public void should_parse_bluray576p_quality(string title)
        {
            ParseAndVerifyQuality(title, Source.BLURAY, false, Resolution.R576p);
        }

        [TestCase("Movie.Title.2016.REMUX.1080p.BluRay.AVC.DTS-HD.MA.5.1-iFT")]
        [TestCase("Movie.Name.2008.REMUX.1080p.Bluray.AVC.DTS-HR.MA.5.1-LEGi0N")]
        [TestCase("Movie.Name.2008.BDREMUX.1080p.Bluray.AVC.DTS-HR.MA.5.1-LEGi0N")]
        [TestCase("Movie.Title.M.2008.USA.BluRay.Remux.1080p.MPEG-2.DD.5.1-TDD")]
        [TestCase("Movie.Title.2018.1080p.BluRay.REMUX.MPEG-2.DTS-HD.MA.5.1-EPSiLON")]
        [TestCase("Movie.Title.II.2003.4K.BluRay.Remux.1080p.AVC.DTS-HD.MA.5.1-BMF")]
        public void should_parse_remux1080p_quality(string title)
        {
            ParseAndVerifyQuality(title, Source.BLURAY, false, Resolution.R1080p, Modifier.REMUX);
        }

        [TestCase("Movie.Title.2016.REMUX.2160p.BluRay.AVC.DTS-HD.MA.5.1-iFT")]
        [TestCase("Movie.Name.2008.REMUX.2160p.Bluray.AVC.DTS-HR.MA.5.1-LEGi0N")]
        [TestCase("Movie.Title.1980.2160p.UHD.BluRay.Remux.HDR.HEVC.DTS-HD.MA.5.1-PmP.mkv")]
        [TestCase("Movie.Title.2016.T1.UHDRemux.2160p.HEVC.Dual.AC3.5.1-TrueHD.5.1.Sub")]
        [TestCase("[Dolby Vision] Movie.Title.S07.MULTi.UHD.BLURAY.REMUX.DV-NoTag")]
        [TestCase("Movie.Name.2020.German.UHDBD.2160p.HDR10.HEVC.EAC3.DL.Remux-pmHD.mkv")]
        public void should_parse_remux2160p_quality(string title)
        {
            ParseAndVerifyQuality(title, Source.BLURAY, false, Resolution.R2160p, Modifier.REMUX);
        }

        [TestCase("Movie.Title.2013.BDISO")]
        [TestCase("Movie.Title.2005.MULTi.COMPLETE.BLURAY-VLS")]
        [TestCase("Movie Name (2012) Bluray ISO [USENET-TURK]")]
        [TestCase("Movie Name.1993..BD25.ISO")]
        [TestCase("Movie.Title.2012.Bluray.1080p.3D.AVC.DTS-HD.MA.5.1.iso")]
        [TestCase("Movie.Title.1996.Bluray.ISO")]
        public void should_parse_brdisk_1080p_quality(string title)
        {
            ParseAndVerifyQuality(title, Source.BLURAY, false, Resolution.R1080p, Modifier.BRDISK);
        }

        [TestCase("Movie.Title.2015.Open.Matte.1080i.HDTV.DD5.1.MPEG2", false)]
        [TestCase("Movie.Title.2009.1080i.HDTV.AAC2.0.MPEG2-PepelefuF", false)]
        public void should_parse_raw_quality(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Source.TV, proper, Resolution.R1080p, Modifier.RAWHD);
        }

        [TestCase("Some.Movie.S02E15", false)]
        [TestCase("Movie Name - 11x11 - Quickie", false)]
        [TestCase("Movie.Name.S01E01.webm", false)]
        [TestCase("Movie.Title.S01E01.The.Web.MT-dd", false)]
        public void quality_parse(string title, bool proper)
        {
            ParseAndVerifyQuality(title, Source.UNKNOWN, proper, Resolution.Unknown);
        }

        /*[Test, TestCaseSource("SelfQualityParserCases")]
        public void parsing_our_own_quality_enum_name(QualityDefinition definition)
        {
            var fileName = string.Format("My series S01E01 [{0}]", definition.Title);
            var result = QualityParser.ParseQuality(fileName);
            var source = definition.QualityTags?.FirstOrDefault(t => t.TagType == TagType.Source)?.Value;
            var resolution = definition.QualityTags?.FirstOrDefault(t => t.TagType == TagType.Resolution)?.Value;
            var modifier = definition.QualityTags?.FirstOrDefault(t => t.TagType == TagType.Modifier)?.Value;
            if (source != null) result.Source.Should().Be(source);
            if (resolution != null) result.Resolution.Should().Be(resolution);
            if (modifier != null) result.Modifier.Should().Be(modifier);

        }*/

        [Test]
        [TestCaseSource("OtherSourceQualityParserCases")]
        public void should_parse_quality_from_other_source(string qualityString, Source source, Resolution resolution, Modifier modifier = Modifier.NONE)
        {
            foreach (var c in new char[] { '-', '.', ' ', '_' })
            {
                var title = string.Format("My series S01E01 {0}", qualityString.Replace(' ', c));

                ParseAndVerifyQuality(title, source, false, resolution, modifier);
            }
        }

        [TestCase("Movie - 2018 [HDTV-1080p]")]
        [TestCase("Movie.Title.S10E09.Eddie.Murphy.The.Honeydrippers.1080i.UPSCALE.HDTV.DD5.1.MPEG2-zebra")]
        [TestCase("Movie.Title.2018.Bluray720p")]
        [TestCase("Movie.Title.2018.Bluray1080p")]
        [TestCase("Movie.Title.2018.Bluray2160p")]
        [TestCase("Movie.Title.2018.848x480.dvd")]
        [TestCase("Movie.Title.2018.848x480.Bluray")]
        [TestCase("Movie.Title.2018.1280x720.Bluray")]
        [TestCase("Movie.Title.2018.1920x1080.Bluray")]
        public void should_parse_full_quality_from_name(string title)
        {
            var result = QualityParser.ParseQuality(title);

            result.SourceDetectionSource.Should().Be(QualityDetectionSource.Name);
            result.ResolutionDetectionSource.Should().Be(QualityDetectionSource.Name);
        }

        [TestCase("Movie.Title.2018.848x480")]
        [TestCase("Movie.Title.2018.1280x720")]
        [TestCase("Movie.Title.2018.1920x1080")]
        public void should_parse_resolution_from_name(string title)
        {
            var result = QualityParser.ParseQuality(title);

            result.SourceDetectionSource.Should().Be(QualityDetectionSource.Unknown);
            result.ResolutionDetectionSource.Should().Be(QualityDetectionSource.Name);
        }

        [TestCase("Movie.Title.2011.S02E01.WS.PDTV.x264-REPACK-TLA")]
        [TestCase("Movie.Title.S01E01.Bluray")]
        [TestCase("Movie.Title.S01E01.HD.TV")]
        [TestCase("Movie.Title.S01E01.SD.TV")]
        public void should_parse_source_from_name(string title)
        {
            var result = QualityParser.ParseQuality(title);

            result.SourceDetectionSource.Should().Be(QualityDetectionSource.Name);
            result.ResolutionDetectionSource.Should().Be(QualityDetectionSource.Unknown);
        }

        [TestCase("Movie.Title.S01E02.Chained.Heat.mkv")]
        [TestCase("Movie Name - S01E01 - Title.avi")]
        [TestCase("Movie.Title..9x18.sunshine_days.avi")]
        [TestCase("[CR] Movie Title - 004 [48CE2D0F].avi")]
        public void should_parse_quality_from_extension(string title)
        {
            var result = QualityParser.ParseQuality(title);

            result.SourceDetectionSource.Should().Be(QualityDetectionSource.Extension);
            result.ResolutionDetectionSource.Should().Be(QualityDetectionSource.Extension);
        }

        [TestCase("Movie.Name.S01E02.Chained.Heat.1080p.mkv")]
        [TestCase("Movie Name - S01E01 - Title.720p.avi")]
        public void should_parse_resolution_from_name_and_source_from_extension(string title)
        {
            var result = QualityParser.ParseQuality(title);

            result.SourceDetectionSource.Should().Be(QualityDetectionSource.Extension);
            result.ResolutionDetectionSource.Should().Be(QualityDetectionSource.Name);
        }

        [TestCase("Movie.Title.2016.1080p.KORSUB.WEBRip.x264.AAC2.0-RADARR", "KORSUB")]
        [TestCase("Movie.Title.2016.1080p.KORSUBS.WEBRip.x264.AAC2.0-RADARR", "KORSUBS")]
        [TestCase("Movie Title 2017 HC 720p HDRiP DD5 1 x264-LEGi0N", "Generic Hardcoded Subs")]
        [TestCase("Movie.Title.2017.720p.SUBBED.HDRip.V2.XViD-26k.avi", "Generic Hardcoded Subs")]
        [TestCase("Movie Title! 2018 [Web][MKV][h264][480p][AAC 2.0][Softsubs]", null)]
        [TestCase("Movie Title! 2019 [HorribleSubs][Web][MKV][h264][848x480][AAC 2.0][Softsubs(HorribleSubs)]", null)]
        public void should_parse_hardcoded_subs(string postTitle, string sub)
        {
            QualityParser.ParseQuality(postTitle).HardcodedSubs.Should().Be(sub);
        }

        [TestCase("Movie Title 2018 REPACK 720p x264 aAF", true)]
        [TestCase("Movie.Title.2018.REPACK.720p.x264-aAF", true)]
        [TestCase("Movie.Title.2018.PROPER.720p.x264-aAF", false)]
        [TestCase("Movie.Title.2018.RERIP.720p.BluRay.x264-DEMAND", true)]
        public void should_be_able_to_parse_repack(string title, bool isRepack)
        {
            var result = QualityParser.ParseQuality(title);
            result.Revision.Version.Should().Be(2);
            result.Revision.IsRepack.Should().Be(isRepack);
        }

        private void ParseAndVerifyQuality(string title, Source source, bool proper, Resolution resolution, Modifier modifier = Modifier.NONE)
        {
            var result = QualityParser.ParseQuality(title);
            if (resolution != Resolution.Unknown)
            {
                result.Quality.Resolution.Should().Be((int)resolution);
            }

            result.Quality.Source.Should().Be(source);
            if (modifier != Modifier.NONE)
            {
                result.Quality.Modifier.Should().Be(modifier);
            }

            var version = proper ? 2 : 1;
            result.Revision.Version.Should().Be(version);
        }
    }
}
