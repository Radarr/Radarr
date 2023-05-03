import { createSelector } from 'reselect';
import Command from 'Commands/Command';
import { MOVIE_SEARCH, REFRESH_MOVIE } from 'Commands/commandNames';
import Movie from 'Movie/Movie';
import createExecutingCommandsSelector from 'Store/Selectors/createExecutingCommandsSelector';
import createMovieQualityProfileSelector from 'Store/Selectors/createMovieQualityProfileSelector';
import { createMovieSelectorForHook } from 'Store/Selectors/createMovieSelector';

function createMovieIndexItemSelector(movieId: number) {
  return createSelector(
    createMovieSelectorForHook(movieId),
    createMovieQualityProfileSelector(movieId),
    createExecutingCommandsSelector(),
    (movie: Movie, qualityProfile, executingCommands: Command[]) => {
      const isRefreshingMovie = executingCommands.some((command) => {
        return (
          command.name === REFRESH_MOVIE && command.body.movieId === movieId
        );
      });

      const isSearchingMovie = executingCommands.some((command) => {
        return (
          command.name === MOVIE_SEARCH && command.body.movieId === movieId
        );
      });

      return {
        movie,
        qualityProfile,
        isRefreshingMovie,
        isSearchingMovie,
      };
    }
  );
}

export default createMovieIndexItemSelector;
