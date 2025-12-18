using System.Collections.Generic;
using NzbDrone.Core.MediaTypes;

namespace NzbDrone.Core.Indexers
{
    public static class NewznabStandardCategory
    {
        public static readonly int Console = 1000;
        public static readonly int ConsoleNDS = 1010;
        public static readonly int ConsolePSP = 1020;
        public static readonly int ConsoleWii = 1030;
        public static readonly int ConsoleXbox = 1040;
        public static readonly int ConsoleXbox360 = 1050;
        public static readonly int ConsoleWiiware = 1060;
        public static readonly int ConsoleXbox360DLC = 1070;
        public static readonly int ConsolePS3 = 1080;
        public static readonly int ConsoleOther = 1090;
        public static readonly int Console3DS = 1110;
        public static readonly int ConsolePSVita = 1120;
        public static readonly int ConsoleWiiU = 1130;
        public static readonly int ConsoleXboxOne = 1140;
        public static readonly int ConsolePS4 = 1180;

        public static readonly int Movies = 2000;
        public static readonly int MoviesForeign = 2010;
        public static readonly int MoviesOther = 2020;
        public static readonly int MoviesSD = 2030;
        public static readonly int MoviesHD = 2040;
        public static readonly int MoviesUHD = 2045;
        public static readonly int MoviesBluRay = 2050;
        public static readonly int Movies3D = 2060;
        public static readonly int MoviesDVD = 2070;
        public static readonly int MoviesWEBDL = 2080;

        public static readonly int Audio = 3000;
        public static readonly int AudioMP3 = 3010;
        public static readonly int AudioVideo = 3020;
        public static readonly int AudioAudiobook = 3030;
        public static readonly int AudioLossless = 3040;
        public static readonly int AudioOther = 3050;
        public static readonly int AudioForeign = 3060;

        public static readonly int PC = 4000;
        public static readonly int PC0day = 4010;
        public static readonly int PCISO = 4020;
        public static readonly int PCMac = 4030;
        public static readonly int PCMobileOther = 4040;
        public static readonly int PCGames = 4050;
        public static readonly int PCMobileiOS = 4060;
        public static readonly int PCMobileAndroid = 4070;

        public static readonly int TV = 5000;
        public static readonly int TVWEBDL = 5010;
        public static readonly int TVForeign = 5020;
        public static readonly int TVSD = 5030;
        public static readonly int TVHD = 5040;
        public static readonly int TVUHD = 5045;
        public static readonly int TVOther = 5050;
        public static readonly int TVSport = 5060;
        public static readonly int TVAnime = 5070;
        public static readonly int TVDocumentary = 5080;

        public static readonly int XXX = 6000;
        public static readonly int XXXDVD = 6010;
        public static readonly int XXXWMV = 6020;
        public static readonly int XXXXviD = 6030;
        public static readonly int XXXx264 = 6040;
        public static readonly int XXXOther = 6050;
        public static readonly int XXXImageset = 6060;
        public static readonly int XXXPacks = 6070;

        public static readonly int Books = 7000;
        public static readonly int BooksMags = 7010;
        public static readonly int BooksEBook = 7020;
        public static readonly int BooksComics = 7030;
        public static readonly int BooksTechnical = 7040;
        public static readonly int BooksOther = 7050;
        public static readonly int BooksForeign = 7060;

        public static readonly int Other = 8000;
        public static readonly int OtherMisc = 8010;
        public static readonly int OtherHashed = 8020;

        public static IReadOnlyList<int> GetCategoriesForMediaType(MediaType mediaType)
        {
            return mediaType switch
            {
                MediaType.Movie => new[]
                {
                    Movies, MoviesForeign, MoviesOther, MoviesSD, MoviesHD,
                    MoviesUHD, MoviesBluRay, Movies3D, MoviesDVD, MoviesWEBDL
                },
                MediaType.TV => new[]
                {
                    TV, TVWEBDL, TVForeign, TVSD, TVHD, TVUHD,
                    TVOther, TVSport, TVAnime, TVDocumentary
                },
                MediaType.Music => new[]
                {
                    Audio, AudioMP3, AudioVideo, AudioLossless, AudioOther, AudioForeign
                },
                MediaType.Audiobook => new[] { AudioAudiobook, Audio },
                MediaType.Book => new[]
                {
                    Books, BooksMags, BooksEBook, BooksTechnical, BooksOther, BooksForeign
                },
                MediaType.Comic => new[] { BooksComics, Books },
                MediaType.Podcast => new[] { Audio, AudioOther },
                _ => new[] { Movies }
            };
        }

        public static IReadOnlyList<int> GetIgnoredCategoriesForMediaType(MediaType mediaType)
        {
            return mediaType switch
            {
                MediaType.Movie => new[] { Console, Audio, PC, XXX, Books },
                MediaType.TV => new[] { Console, Audio, PC, Movies, XXX, Books },
                MediaType.Music => new[] { Console, PC, Movies, XXX, Books, TV },
                MediaType.Audiobook => new[] { Console, PC, Movies, XXX, TV },
                MediaType.Book => new[] { Console, Audio, PC, Movies, XXX, TV },
                MediaType.Comic => new[] { Console, Audio, PC, Movies, XXX, TV },
                MediaType.Podcast => new[] { Console, PC, Movies, XXX, Books, TV },
                _ => new[] { Console, Audio, PC, XXX, Books }
            };
        }
    }
}
