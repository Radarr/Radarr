using System.Collections.Generic;
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
        private List<Album> _albums = new List<Album> { new Album() };

        [SetUp]
        public void Setup()
        {
            _artist = Builder<Artist>
                .CreateNew()
                .Build();
            _albums = Builder<List<Album>>
                .CreateNew()
                .Build();
        }

        private void GivenSearchCriteria(string artistName, string albumTitle)
        {
            _artist.Name = artistName;
            var a = new Album();
            a.Title = albumTitle;
            _albums.Add(a);
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

        [TestCase("Songs of Experience (Deluxe Edition)", "Songs of Experience")]
        [TestCase("Songs of Experience (iTunes Deluxe Edition)", "Songs of Experience")]
        [TestCase("Songs of Experience [Super Special Edition]", "Songs of Experience")]
        [TestCase("Mr. Bad Guy [Special Edition]", "Mr. Bad Guy")]
        [TestCase("Sweet Dreams (Album)", "Sweet Dreams")]
        [TestCase("Now What?! (Limited Edition)", "Now What?!")]
        [TestCase("Random Album Title (Promo CD)", "Random Album Title")]
        [TestCase("Hello, I Must Be Going (2016 Remastered)", "Hello, I Must Be Going")]
        [TestCase("Limited Edition", "Limited Edition")]
        public void should_remove_common_tags_from_album_title(string title, string correct)
        {
            var result = Parser.Parser.CleanAlbumTitle(title);
            result.Should().Be(correct);
        }

        [TestCase("Songs of Experience (Deluxe Edition)", "Songs of Experience")]
        [TestCase("Mr. Bad Guy [Special Edition]", "Mr. Bad Guy")]
        [TestCase("Smooth Criminal (single)", "Smooth Criminal")]
        [TestCase("Wie Maak Die Jol Vol (Ft. Isaac Mutant, Knoffel, Jaak Paarl & Scallywag)", "Wie Maak Die Jol Vol")]
        [TestCase("Alles Schon Gesehen (Feat. Deichkind)", "Alles Schon Gesehen")]
        [TestCase("Science Fiction/Double Feature", "Science Fiction/Double Feature")]
        [TestCase("Dancing Feathers", "Dancing Feathers")]
        public void should_remove_common_tags_from_track_title(string title, string correct)
        {
            var result = Parser.Parser.CleanTrackTitle(title);
            result.Should().Be(correct);
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

        [TestCase("02 Unchained.flac")] // This isn't valid on any regex we have. We must always have an artist
        [TestCase("Fall Out Boy - 02 - Title.wav")] // This isn't valid on any regex we have. We don't support Artist - Track - TrackName
        [Ignore("Ignore Test until track parsing rework")]
        public void should_parse_quality_from_extension(string title)
        {
            Parser.Parser.ParseAlbumTitle(title).Quality.Quality.Should().NotBe(Quality.Unknown);
            Parser.Parser.ParseAlbumTitle(title).Quality.QualityDetectionSource.Should().Be(QualityDetectionSource.Extension);
        }

        [TestCase("VA - The Best 101 Love Ballads (2017) MP3 [192 kbps]", "VA", "The Best 101 Love Ballads")]
        [TestCase("ATCQ - The Love Movement 1998 2CD 192kbps  RIP", "ATCQ", "The Love Movement")]
        //[TestCase("A Tribe Called Quest - The Love Movement 1998 2CD [192kbps] RIP", "A Tribe Called Quest", "The Love Movement")]
        [TestCase("Maula - Jism 2 [2012] Mp3 - 192Kbps [Extended]- TK", "Maula", "Jism 2")]
        [TestCase("VA - Complete Clubland - The Ultimate Ride Of Your Lfe [2014][MP3][192 kbps]", "VA", "Complete Clubland - The Ultimate Ride Of Your Lfe")]
        [TestCase("Complete Clubland - The Ultimate Ride Of Your Lfe [2014][MP3](192kbps)", "Complete Clubland", "The Ultimate Ride Of Your Lfe")]
        //[TestCase("The Ultimate Ride Of Your Lfe [192 KBPS][2014][MP3]", "", "The Ultimate Ride Of Your Lfe")]
        [TestCase("Gary Clark Jr - Live North America 2016 (2017) MP3 192kbps", "Gary Clark Jr", "Live North America 2016")]
        //[TestCase("Beyoncé Lemonade [320] 2016 Beyonce Lemonade [320] 2016", "Beyoncé", "Lemonade")]
        [TestCase("Childish Gambino - Awaken, My Love Album 2016 mp3 320 Kbps", "Childish Gambino", "Awaken, My Love Album")]
        //[TestCase("Maluma – Felices Los 4 MP3 320 Kbps 2017 Download", "Maluma", "Felices Los 4")]
        [TestCase("Ricardo Arjona - APNEA (Single 2014) (320 kbps)", "Ricardo Arjona", "APNEA")]
        [TestCase("Kehlani - SweetSexySavage (Deluxe Edition) (2017) 320", "Kehlani", "SweetSexySavage")]
        [TestCase("Anderson Paak - Malibu (320)(2016)", "Anderson Paak", "Malibu")]
        [TestCase("Caetano Veloso Discografia Completa MP3 @256", "Caetano Veloso", "Discography", true)]
        [TestCase("Little Mix - Salute [Deluxe Edition] [2013] [M4A-256]-V3nom [GLT", "Little Mix", "Salute")]
        [TestCase("Ricky Martin - A Quien Quiera Escuchar (2015) 256 kbps [GloDLS]", "Ricky Martin", "A Quien Quiera Escuchar")]
        [TestCase("Jake Bugg - Jake Bugg (Album) [2012] {MP3 256 kbps}", "Jake Bugg", "Jake Bugg")]
        [TestCase("Milky Chance - Sadnecessary [256 Kbps] [M4A]", "Milky Chance", "Sadnecessary")]
        [TestCase("Clean Bandit - New Eyes [2014] [Mp3-256]-V3nom [GLT]", "Clean Bandit", "New Eyes")]
        [TestCase("Armin van Buuren - A State Of Trance 810 (20.04.2017) 256 kbps", "Armin van Buuren", "A State Of Trance 810")]
        [TestCase("PJ Harvey - Let England Shake [mp3-256-2011][trfkad]", "PJ Harvey", "Let England Shake")]
        //[TestCase("X-Men Soundtracks (2006-2014) AAC, 256 kbps", "", "")]
        //[TestCase("Walk the Line Soundtrack (2005) [AAC, 256 kbps]", "", "Walk the Line Soundtrack")]
        //[TestCase("Emeli Sande Next To Me (512 Kbps)", "Emeli", "Next To Me")]
        [TestCase("Kendrick Lamar - DAMN (2017) FLAC", "Kendrick Lamar", "DAMN")]
        [TestCase("Alicia Keys - Vault Playlist Vol. 1 (2017) [FLAC CD]", "Alicia Keys", "Vault Playlist Vol  1")]
        [TestCase("Gorillaz - Humanz (Deluxe) - lossless FLAC Tracks - 2017 - CDrip", "Gorillaz", "Humanz")]
        [TestCase("David Bowie - Blackstar (2016) [FLAC]", "David Bowie", "Blackstar")]
        [TestCase("The Cure - Greatest Hits (2001) FLAC Soup", "The Cure", "Greatest Hits")]
        [TestCase("Slowdive - Souvlaki (FLAC)", "Slowdive", "Souvlaki")]
        [TestCase("John Coltrane - Kulu Se Mama (1965) [EAC-FLAC]", "John Coltrane", "Kulu Se Mama")]
        [TestCase("The Rolling Stones - The Very Best Of '75-'94 (1995) {FLAC}", "The Rolling Stones", "The Very Best Of '75-'94")]
        [TestCase("Migos-No_Label_II-CD-FLAC-2014-FORSAKEN", "Migos", "No Label II")]
        //[TestCase("ADELE 25 CD FLAC 2015 PERFECT", "Adele", "25")]
        [TestCase("A.I. - Sex & Robots [2007/MP3/V0(VBR)]", "A I", "Sex & Robots")]
        [TestCase("Jay-Z - 4:44 (Deluxe Edition) (2017) 320", "Jay-Z", "444")]
        //[TestCase("Roberta Flack 2006 - The Very Best of", "Roberta Flack", "The Very Best of")]
        [TestCase("VA - NOW Thats What I Call Music 96 (2017) [Mp3~Kbps]", "VA", "NOW Thats What I Call Music 96")]
        [TestCase("Queen - The Ultimate Best Of Queen(2011)[mp3]", "Queen", "The Ultimate Best Of Queen")]
        [TestCase("Little Mix - Salute [Deluxe Edition] [2013] [M4A-256]-V3nom [GLT]", "Little Mix", "Salute")]
        [TestCase("Barış Manço - Ben Bilirim [1993/FLAC/Lossless/Log]", "Barış Manço", "Ben Bilirim")]
        [TestCase("Imagine Dragons-Smoke And Mirrors-Deluxe Edition-2CD-FLAC-2015-JLM", "Imagine Dragons", "Smoke And Mirrors")]
        [TestCase("Dani_Sbert-Togheter-WEB-2017-FURY", "Dani Sbert", "Togheter")]
        [TestCase("New.Edition-One.Love-CD-FLAC-2017-MrFlac", "New Edition", "One Love")]
        [TestCase("David_Gray-The_Best_of_David_Gray-(Deluxe_Edition)-2CD-2016-MTD", "David Gray", "The Best of David Gray")]
        [TestCase("Shinedown-Us and Them-NMR-2005-NMR", "Shinedown", "Us and Them")]
        [TestCase("Led Zeppelin - Studio Discography 1969-1982 (10 albums)(flac)", "Led Zeppelin", "Discography", true)]
        [TestCase("Minor Threat - Complete Discography [1989] [Anthology]", "Minor Threat", "Discography", true)]
        [TestCase("Captain-Discography_1998_-_2001-CD-FLAC-2007-UTP", "Captain", "Discography", true)]
        [TestCase("Coolio - Gangsta's Paradise (1995) (FLAC Lossless)", "Coolio", "Gangsta's Paradise")]
        [TestCase("Brother Ali-2007-The Undisputed Truth-FTD", "Brother Ali", "The Undisputed Truth")]
        [TestCase("Brother Ali-The Undisputed Truth-2007-FTD", "Brother Ali", "The Undisputed Truth")]

        // ruTracker
        [TestCase("(Eclectic Progressive Rock) [CD] Peter Hammill - From The Trees - 2017, FLAC (tracks + .cue), lossless", "Peter Hammill","From The Trees")]
        [TestCase("(Folk Rock / Pop) Aztec Two-Step - Naked - 2017, MP3, 320 kbps", "Aztec Two-Step", "Naked")]
        [TestCase("(Zeuhl / Progressive Rock) [WEB] Dai Kaht - Dai Kaht - 2017, FLAC (tracks), lossless", "Dai Kaht", "Dai Kaht")]
        //[TestCase("(Industrial Folk) Bumblebee(Shmely, AntiVirus) - Discography, 23 albums - 1998-2011, FLAC(image + .cue), lossless")]
        //[TestCase("(Heavy Metal) Sergey Mavrin(Mavrik) - Discography(14 CD) [1998-2010], FLAC(image + .cue), lossless")]
        [TestCase("(Heavy Metal) [CD] Black Obelisk - Discography - 1991-2015 (36 releases, 32 CDs), FLAC(image + .cue), lossless", "Black Obelisk", "Discography", true)]
        //[TestCase("(R'n'B / Soul) Moyton - One of the Sta(2014) + Ocean(2014), MP3, 320 kbps", "Moyton", "")]
        [TestCase("(Heavy Metal) Aria - Discography(46 CD) [1985 - 2015], FLAC(image + .cue), lossless", "Aria", "Discography", true)]
        [TestCase("(Heavy Metal) [CD] Forces United - Discography(6 CDs), 2014-2016, FLAC(image + .cue), lossless", "Forces United", "Discography", true)]
        [TestCase("Gorillaz - The now now - 2018 [FLAC]", "Gorillaz", "The now now")]

        //Regex Works on below, but ParseAlbumMatchCollection cleans the "..." and converts it to spaces
        // [TestCase("Metallica - ...And Justice for All (1988) [FLAC Lossless]", "Metallica", "...And Justice for All")]
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
        [TestCase("Black.Sabbath-FLAC-Black.Sabbath")]
        [TestCase("Black_Sabbath-FLAC-Black_Sabbath")]
        public void should_parse_artist_name_and_album_title_by_search_criteria(string releaseTitle)
        {
            GivenSearchCriteria("Black Sabbath", "Black Sabbath");
            var parseResult = Parser.Parser.ParseAlbumTitleWithSearchCriteria(releaseTitle, _artist, _albums);
            parseResult.ArtistName.ToLowerInvariant().Should().Be("black sabbath");
            parseResult.AlbumTitle.ToLowerInvariant().Should().Be("black sabbath");
        }

        [TestCase("Captain-Discography_1998_-_2001-CD-FLAC-2007-UTP", 1998, 2001)]
        [TestCase("(Heavy Metal) Aria - Discography(46 CD) [1985 - 2015]", 1985, 2015)]
        [TestCase("Led Zeppelin - Studio Discography 1969-1982 (10 albums)(flac)", 1969, 1982)]
        [TestCase("Minor Threat - Complete Discography [1989] [Anthology]", 0, 1989)]
        [TestCase("Caetano Veloso Discografia Completa MP3 @256", 0, 0)]
        public void should_parse_year_or_year_range_from_discography(string releaseTitle, int startyear,
            int endyear)
        {
            var parseResult = Parser.Parser.ParseAlbumTitle(releaseTitle);
            parseResult.Discography.Should().BeTrue();
            parseResult.DiscographyStart.Should().Be(startyear);
            parseResult.DiscographyEnd.Should().Be(endyear);
        }

        [Test]
        public void should_not_parse_artist_name_and_album_title_by_incorrect_search_criteria()
        {
            GivenSearchCriteria("Abba", "Abba");
            var parseResult = Parser.Parser.ParseAlbumTitleWithSearchCriteria("Black Sabbath  Black Sabbath FLAC", _artist, _albums);
            parseResult.Should().BeNull();
        }

        [TestCase("Ed Sheeran", "I See Fire", "Ed Sheeran I See Fire[Mimp3.eu].mp3 FLAC")]
        [TestCase("Ed Sheeran", "Divide", "Ed Sheeran   ? Divide FLAC")]
        [TestCase("Ed Sheeran", "+", "Ed Sheeran + FLAC")]
        //[TestCase("Glasvegas", @"EUPHORIC /// HEARTBREAK \\\", @"EUPHORIC /// HEARTBREAK \\\ FLAC")] // slashes not being escaped properly
        [TestCase("XXXTENTACION", "?", "XXXTENTACION ? FLAC")]
        [TestCase("Hey", "BŁYSK", "Hey - BŁYSK FLAC")]
        public void should_escape_albums(string artist, string album, string releaseTitle)
        {
            GivenSearchCriteria(artist, album);
            var parseResult = Parser.Parser.ParseAlbumTitleWithSearchCriteria(releaseTitle, _artist, _albums);
            parseResult.AlbumTitle.Should().Be(album);
        }

        [TestCase("???", "Album", "??? Album FLAC")]
        [TestCase("+", "Album", "+ Album FLAC")]
        [TestCase(@"/\", "Album", @"/\ Album FLAC")]
        [TestCase("+44", "When Your Heart Stops Beating", "+44 When Your Heart Stops Beating FLAC")]
        public void should_escape_artists(string artist, string album, string releaseTitle)
        {
            GivenSearchCriteria(artist, album);
            var parseResult = Parser.Parser.ParseAlbumTitleWithSearchCriteria(releaseTitle, _artist, _albums);
            parseResult.ArtistName.Should().Be(artist);
        }

        [TestCase("Michael Bubl\u00E9", "Michael Bubl\u00E9", @"Michael Buble Michael Buble CD FLAC 2003 PERFECT")]
        public void should_match_with_accent_in_artist_and_album(string artist, string album, string releaseTitle)
        {
            GivenSearchCriteria(artist, album);
            var parseResult = Parser.Parser.ParseAlbumTitleWithSearchCriteria(releaseTitle, _artist, _albums);
            parseResult.ArtistName.Should().Be("Michael Buble");
            parseResult.AlbumTitle.Should().Be("Michael Buble");
        }

        [Test]
        public void should_find_result_if_multiple_albums_in_searchcriteria()
        {
            GivenSearchCriteria("Michael Bubl\u00E9", "Call Me Irresponsible");
            GivenSearchCriteria("Michael Bubl\u00E9", "Michael Bubl\u00E9");
            GivenSearchCriteria("Michael Bubl\u00E9", "love");
            GivenSearchCriteria("Michael Bubl\u00E9", "Christmas");
            GivenSearchCriteria("Michael Bubl\u00E9", "To Be Loved");
            var parseResult = Parser.Parser.ParseAlbumTitleWithSearchCriteria(
                "Michael Buble Christmas (Deluxe Special Edition) CD FLAC 2012 UNDERTONE iNT", _artist, _albums);
            parseResult.ArtistName.Should().Be("Michael Buble");
            parseResult.AlbumTitle.Should().Be("Christmas");
        }
    }
}
