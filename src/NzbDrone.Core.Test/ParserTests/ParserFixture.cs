using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{

    [TestFixture]
    public class ParserFixture : CoreTest
    {
        /*Fucked-up hall of shame,
         * WWE.Wrestlemania.27.PPV.HDTV.XviD-KYR
         * Unreported.World.Chinas.Lost.Sons.WS.PDTV.XviD-FTP
         * [TestCase("Big Time Rush 1x01 to 10 480i DD2 0 Sianto", "Big Time Rush", 1, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, 10)]
         * [TestCase("Desparate Housewives - S07E22 - 7x23 - And Lots of Security.. [HDTV-720p].mkv", "Desparate Housewives", 7, new[] { 22, 23 }, 2)]
         * [TestCase("S07E22 - 7x23 - And Lots of Security.. [HDTV-720p].mkv", "", 7, new[] { 22, 23 }, 2)]
         * (Game of Thrones s03 e - "Game of Thrones Season 3 Episode 10"
         * The.Man.of.Steel.1994-05.33.hybrid.DreamGirl-Novus-HD
         * Superman.-.The.Man.of.Steel.1994-06.34.hybrid.DreamGirl-Novus-HD
         * Superman.-.The.Man.of.Steel.1994-05.33.hybrid.DreamGirl-Novus-HD
         * Constantine S1-E1-WEB-DL-1080p-NZBgeek
         */

        [TestCase("Chuck - 4x05 - Title", "Chuck")]
        [TestCase("Law & Order - 4x05 - Title", "laworder")]
        [TestCase("Bad Format", "badformat")]
        [TestCase("Mad Men - Season 1 [Bluray720p]", "madmen")]
        [TestCase("Mad Men - Season 1 [Bluray1080p]", "madmen")]
        [TestCase("The Daily Show With Jon Stewart -", "thedailyshowwithjonstewart")]
        [TestCase("The Venture Bros. (2004)", "theventurebros2004")]
        [TestCase("Castle (2011)", "castle2011")]
        [TestCase("Adventure Time S02 720p HDTV x264 CRON", "adventuretime")]
        [TestCase("Hawaii Five 0", "hawaiifive0")]
        [TestCase("Match of the Day", "matchday")]
        [TestCase("Match of the Day 2", "matchday2")]
        [TestCase("[ www.Torrenting.com ] - Revenge.S03E14.720p.HDTV.X264-DIMENSION", "Revenge")]
        [TestCase("Seed S02E09 HDTV x264-2HD [eztv]-[rarbg.com]", "Seed")]
        [TestCase("Reno.911.S01.DVDRip.DD2.0.x264-DEEP", "Reno 911")]
        public void should_parse_artist_name(string postTitle, string title)
        {
            var result = Parser.Parser.ParseArtistName(postTitle).CleanArtistName();
            result.Should().Be(title.CleanArtistName());
        }

        [Test]
        public void should_remove_accents_from_title()
        {
            const string title = "Carniv\u00E0le";
            
            title.CleanArtistName().Should().Be("carnivale");
        }

        [TestCase("Discovery TV - Gold Rush : 02 Road From Hell [S04].mp4")]
        public void should_clean_up_invalid_path_characters(string postTitle)
        {
            Parser.Parser.ParseAlbumTitle(postTitle);
        }

        [TestCase("[scnzbefnet][509103] 2.Broke.Girls.S03E18.720p.HDTV.X264-DIMENSION", "2 Broke Girls")]
        public void should_remove_request_info_from_title(string postTitle, string title)
        {
            Parser.Parser.ParseAlbumTitle(postTitle).ArtistName.Should().Be(title);
        }

        [TestCase("Revolution.S01E02.Chained.Heat.mkv")]
        [TestCase("Dexter - S01E01 - Title.avi")]
        public void should_parse_quality_from_extension(string title)
        {
            Parser.Parser.ParseAlbumTitle(title).Quality.Quality.Should().NotBe(Quality.Unknown);
            Parser.Parser.ParseAlbumTitle(title).Quality.QualitySource.Should().Be(QualitySource.Extension);
        }

        [TestCase("VA - The Best 101 Love Ballads (2017) MP3 [192 kbps]", "VA", "The Best 101 Love Ballads")]
        [TestCase("ATCQ - The Love Movement 1998 2CD 192kbps  RIP", "ATCQ", "The Love Movement")]
        [TestCase("A Tribe Called Quest - The Love Movement 1998 2CD [192kbps] RIP", "A Tribe Called Quest", "The Love Movement")]
        [TestCase("Maula - Jism 2 [2012] Mp3 - 192Kbps [Extended]- TK", "Maula", "Jism 2")]
        [TestCase("VA - Complete Clubland - The Ultimate Ride Of Your Lfe [2014][MP3][192 kbps]", "VA", "")]
        [TestCase("Complete Clubland - The Ultimate Ride Of Your Lfe [2014][MP3](192kbps)", "Complete Clubland", "The Ultimate Ride Of Your Lfe")]
        [TestCase("The Ultimate Ride Of Your Lfe [192 KBPS][2014][MP3]", "", "The Ultimate Ride Of Your Lfe")]
        [TestCase("Gary Clark Jr - Live North America 2016 (2017) MP3 192kbps", "Gary Clark Jr", "Live North America 2016")]
        [TestCase("Beyoncé Lemonade [320] 2016 Beyonce Lemonade [320] 2016", "Beyoncé", "Lemonade")]
        [TestCase("Childish Gambino - Awaken, My Love Album 2016 mp3 320 Kbps", "Childish Gambino", "Awaken, My Love Album")]
        [TestCase("Maluma – Felices Los 4 MP3 320 Kbps 2017 Download", "Maluma", "Felices Los 4")]
        [TestCase("Ricardo Arjona - APNEA (Single 2014) (320 kbps)", "Ricardo Arjona", "APNEA")]
        [TestCase("Kehlani - SweetSexySavage (Deluxe Edition) (2017) 320", "Kehlani", "SweetSexySavage")]
        [TestCase("Anderson Paak - Malibu (320)(2016)", "Anderson Paak", "Malibu")]
        [TestCase("Caetano Veloso Discografia Completa MP3 @256", "Caetano Veloso", "")]
        [TestCase("Little Mix - Salute [Deluxe Edition] [2013] [M4A-256]-V3nom [GLT", "Little Mix", "Salute")]
        [TestCase("Ricky Martin - A Quien Quiera Escuchar (2015) 256 kbps [GloDLS]", "Ricky Martin", "A Quien Quiera Escuchar")]
        [TestCase("Jake Bugg - Jake Bugg (Album) [2012] {MP3 256 kbps}", "Jake Bugg", "Jake Bugg")]
        [TestCase("Milky Chance - Sadnecessary [256 Kbps] [M4A]", "Milky Chance", "Sadnecessary")]
        [TestCase("Clean Bandit - New Eyes [2014] [Mp3-256]-V3nom [GLT]", "Clean Bandit", "New Eyes")]
        [TestCase("Armin van Buuren - A State Of Trance 810 (20.04.2017) 256 kbps", "Armin van Buuren", "A State Of Trance 810")]
        [TestCase("PJ Harvey - Let England Shake [mp3-256-2011][trfkad]", "PJ Harvey", "Let England Shake")]
        [TestCase("X-Men Soundtracks (2006-2014) AAC, 256 kbps", "", "")]
        [TestCase("Walk the Line Soundtrack (2005) [AAC, 256 kbps]", "", "Walk the Line Soundtrack")]
        [TestCase("Emeli Sande Next To Me (512 Kbps)", "Emeli", "Next To Me")]
        [TestCase("Kendrick Lamar - DAMN (2017) FLAC", "Kendrick Lamar", "DAMN")]
        [TestCase("Alicia Keys - Vault Playlist Vol. 1 (2017) [FLAC CD]", "Alicia Keys", "Vault Playlist Vol  1")]
        [TestCase("Gorillaz - Humanz (Deluxe) - lossless FLAC Tracks - 2017 - CDrip", "Gorillaz", "Humanz")]
        [TestCase("David Bowie - Blackstar (2016) [FLAC]", "David Bowie", "Blackstar")]
        [TestCase("The Cure - Greatest Hits (2001) FLAC Soup", "The Cure", "Greatest Hits")]
        [TestCase("Slowdive - Souvlaki (FLAC)", "Slowdive", "Souvlaki")]
        [TestCase("John Coltrane - Kulu Se Mama (1965) [EAC-FLAC]", "John Coltrane", "Kulu Se Mama")]
        [TestCase("The Rolling Stones - The Very Best Of '75-'94 (1995) {FLAC}", "The Rolling Stones", "The Very Best Of '75-'94")]
        [TestCase("Migos-No_Label_II-CD-FLAC-2014-FORSAKEN", "Migos", "No Label II")]
        [TestCase("ADELE 25 CD FLAC 2015 PERFECT", "Adele", "25")]
        [TestCase("A.I. - Sex & Robots [2007/MP3/V0(VBR)]", "A.I.", "Sex & Robots")]
        [TestCase("Jay-Z - 4:44 (Deluxe Edition) (2017) 320", "Jay-Z", "444")]
        [TestCase("Roberta Flack 2006 - The Very Best of", "Roberta Flack", "The Very Best of")]
        [TestCase("VA - NOW Thats What I Call Music 96 (2017) [Mp3~Kbps]", "VA", "NOW Thats What I Call Music 96")]
        [TestCase("Queen - The Ultimate Best Of Queen(2011)[mp3]", "Queen", "The Ultimate Best Of Queen")]
        [TestCase("Little Mix - Salute [Deluxe Edition] [2013] [M4A-256]-V3nom [GLT]", "Little Mix", "Salute")]
        [TestCase("Barış Manço - Ben Bilirim [1993/FLAC/Lossless/Log]", "Barış Manço", "Ben Bilirim")]
        [TestCase("Imagine Dragons-Smoke And Mirrors-Deluxe Edition-2CD-FLAC-2015-JLM", "Imagine Dragons", "Smoke And Mirrors")]
        [TestCase("Dani_Sbert-Togheter-WEB-2017-FURY", "Dani Sbert", "Togheter")]
        [TestCase("New.Edition-One.Love-CD-FLAC-2017-MrFlac", "New Edition", "One Love")]
        [TestCase("David_Gray-The_Best_of_David_Gray-(Deluxe_Edition)-2CD-2016-MTD", "David Gray", "The Best of David Gray")]
        public void should_parse_artist_name_and_album_title(string postTitle, string name, string title)
        {

            var parseResult = Parser.Parser.ParseAlbumTitle(postTitle);
            parseResult.ArtistName.Should().Be(name);
            parseResult.AlbumTitle.Should().Be(title);
        }
    }
}
