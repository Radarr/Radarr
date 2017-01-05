using System;
using System.Linq;
using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;


namespace NzbDrone.Core.Tv
{
    public interface IMovieRepository : IBasicRepository<Movie>
    {
        bool MoviePathExists(string path);
        Movie FindByTitle(string cleanTitle);
        Movie FindByTitle(string cleanTitle, int year);
        Movie FindByImdbId(string imdbid);
        List<Movie> GetMoviesByFileId(int fileId);
        void SetFileId(int fileId, int movieId);
    }

    public class MovieRepository : BasicRepository<Movie>, IMovieRepository
    {
        private readonly Dictionary<string, string> romanNumeralsMapper = new Dictionary<string, string>
        {
            { "1", "I"},
            { "2", "II"},
            { "3", "III"},
            { "4", "IV"},
            { "5", "V"},
            { "6", "VI"},
            { "7", "VII"},
            { "8", "VII"},
            { "9", "IX"},
            { "10", "X"},

        }; //If a movie has more than 10 parts fuck 'em.

        public MovieRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public bool MoviePathExists(string path)
        {
            return Query.Where(c => c.Path == path).Any();
        }

        public Movie FindByTitle(string cleanTitle)
        {
            cleanTitle = cleanTitle.ToLowerInvariant();

            var cleanRoman = cleanTitle;

            var cleanNum = cleanTitle;

            foreach (KeyValuePair<string, string> entry in romanNumeralsMapper)
            {
                string num = entry.Key;
                string roman = entry.Value.ToLower();

                cleanRoman = cleanRoman.Replace(num, roman);

                cleanNum = cleanNum.Replace(roman, num);
            }

            var result = Query.Where(s => s.CleanTitle == cleanTitle).SingleOrDefault();

            if (result == null)
            {
                result = Query.Where(s => s.CleanTitle == cleanNum).OrWhere(s => s.CleanTitle == cleanRoman).SingleOrDefault();

                if (result == null)
                {
                    var movies = this.All();

                    result = movies.Where(m => m.AlternativeTitles.Any(t => Parser.Parser.CleanSeriesTitle(t.ToLower()) == cleanTitle ||
                    Parser.Parser.CleanSeriesTitle(t.ToLower()) == cleanRoman ||
                    Parser.Parser.CleanSeriesTitle(t.ToLower()) == cleanNum)).SingleOrDefault();

                    return result;
                }
                else
                {
                    return result;
                }

            }
            else
            {
                return result;
            }
        }

        public Movie FindByTitle(string cleanTitle, int year)
        {
            cleanTitle = cleanTitle.ToLowerInvariant();

            return Query.Where(s => s.CleanTitle == cleanTitle)
                        .AndWhere(s => s.Year == year)
                        .SingleOrDefault();
        }

        public Movie FindByImdbId(string imdbid)
        {
            return Query.Where(s => s.ImdbId == imdbid).SingleOrDefault();
        }

        public List<Movie> GetMoviesByFileId(int fileId)
        {
            return Query.Where(m => m.MovieFileId == fileId).ToList();
        }

        public void SetFileId(int episodeId, int fileId)
        {
            SetFields(new Movie { Id = episodeId, MovieFileId = fileId }, movie => movie.MovieFileId);
        }

    }
}