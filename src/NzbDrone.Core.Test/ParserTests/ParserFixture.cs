using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Music;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{

    [TestFixture]
    public class ParserFixture : CoreTest
    {
        Artist _artist = new Artist();
        private List<Album> _albums = new List<Album>{new Album()};

        [SetUp]
        public void Setup()
        {
            _artist = Builder<Artist>
                .CreateNew()
                .Build();
        }

        private void GivenSearchCriteria(string artistName, string albumTitle)
        {
            _artist.Name = artistName;
            _albums.First().Title = albumTitle;
        }

        [TestCase("Bad Format", "badformat")]
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

        [TestCase("[scnzbefnet][509103] Jay-Z - 4:44 (Deluxe Edition) (2017) 320", "Jay-Z")]
        public void should_remove_request_info_from_title(string postTitle, string title)
        {
            Parser.Parser.ParseAlbumTitle(postTitle).ArtistName.Should().Be(title);
        }

        [TestCase("02 Unchained.flac")]
        [TestCase("Fall Out Boy - 02 - Title.wav")]
        public void should_parse_quality_from_extension(string title)
        {
            Parser.Parser.ParseAlbumTitle(title).Quality.Quality.Should().NotBe(Quality.Unknown);
            Parser.Parser.ParseAlbumTitle(title).Quality.QualitySource.Should().Be(QualitySource.Extension);
        }

        [TestCase("VA - The Best 101 Love Ballads (2017) MP3 [192 kbps]", "VA", "The Best 101 Love Ballads")]
        [TestCase("ATCQ - The Love Movement 1998 2CD 192kbps  RIP", "ATCQ", "The Love Movement")]
        [TestCase("A Tribe Called Quest - The Love Movement 1998 2CD [192kbps] RIP", "A Tribe Called Quest", "The Love Movement")]
        [TestCase("Maula - Jism 2 [2012] Mp3 - 192Kbps [Extended]- TK", "Maula", "Jism 2")]
        [TestCase("VA - Complete Clubland - The Ultimate Ride Of Your Lfe [2014][MP3][192 kbps]", "VA", "Complete Clubland - The Ultimate Ride Of Your Lfe")]
        [TestCase("Complete Clubland - The Ultimate Ride Of Your Lfe [2014][MP3](192kbps)", "Complete Clubland", "The Ultimate Ride Of Your Lfe")]
        [TestCase("The Ultimate Ride Of Your Lfe [192 KBPS][2014][MP3]", "", "The Ultimate Ride Of Your Lfe")]
        [TestCase("Gary Clark Jr - Live North America 2016 (2017) MP3 192kbps", "Gary Clark Jr", "Live North America 2016")]
        [TestCase("Beyoncé Lemonade [320] 2016 Beyonce Lemonade [320] 2016", "Beyoncé", "Lemonade")]
        [TestCase("Childish Gambino - Awaken, My Love Album 2016 mp3 320 Kbps", "Childish Gambino", "Awaken, My Love Album")]
        [TestCase("Maluma – Felices Los 4 MP3 320 Kbps 2017 Download", "Maluma", "Felices Los 4")]
        [TestCase("Ricardo Arjona - APNEA (Single 2014) (320 kbps)", "Ricardo Arjona", "APNEA")]
        [TestCase("Kehlani - SweetSexySavage (Deluxe Edition) (2017) 320", "Kehlani", "SweetSexySavage")]
        [TestCase("Anderson Paak - Malibu (320)(2016)", "Anderson Paak", "Malibu")]
        [TestCase("Caetano Veloso Discografia Completa MP3 @256", "Caetano Veloso", "", true)]
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
        [TestCase("A.I. - Sex & Robots [2007/MP3/V0(VBR)]", "A I", "Sex & Robots")]
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
        [TestCase("Shinedown-Us and Them-NMR-2005-NMR", "Shinedown", "Us and Them")]
        [TestCase("Captain-Discography_1998_-_2001-CD-FLAC-2007-UTP", "Captain", "", true)]
        [TestCase("Coolio - Gangsta's Paradise (1995) (FLAC Lossless)", "Coolio", "Gangsta's Paradise")]
        public void should_parse_artist_name_and_album_title(string postTitle, string name, string title, bool discography = false)
        {

            var parseResult = Parser.Parser.ParseAlbumTitle(postTitle);
            parseResult.ArtistName.Should().Be(name);
            parseResult.AlbumTitle.Should().Be(title);
            parseResult.Discography.Should().Be(discography);
        }

        [TestCase("Black Sabbath - Black Sabbath FLAC")]
        [TestCase("Black Sabbath Black Sabbath FLAC")]
        [TestCase("BlaCk SabBaTh Black SabBatH FLAC")]
        [TestCase("Black Sabbath FLAC Black Sabbath")]
        public void should_parse_artist_name_and_album_title_by_search_criteria(string releaseTitle)
        {
            GivenSearchCriteria("Black Sabbath", "Black Sabbath");
            var parseResult = Parser.Parser.ParseAlbumTitleWithSearchCriteria(releaseTitle, _artist, _albums);
            parseResult.ArtistName.ToLowerInvariant().Should().Be("black sabbath");
            parseResult.AlbumTitle.ToLowerInvariant().Should().Be("black sabbath");
        }

        [Test]
        public void should_not_parse_artist_name_and_album_title_by_incorrect_search_criteria()
        {
            GivenSearchCriteria("Abba", "Abba");
            var parseResult = Parser.Parser.ParseAlbumTitleWithSearchCriteria("Black Sabbath  Black Sabbath FLAC", _artist, _albums);
            parseResult.Should().BeNull();
        }
    }
}
