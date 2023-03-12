import { createSelector } from 'reselect';
import { MOVIE_SEARCH, REFRESH_MOVIE } from 'Commands/commandNames';
import createExecutingCommandsSelector from 'Store/Selectors/createExecutingCommandsSelector';
import createMovieQualityProfileSelector from 'Store/Selectors/createMovieQualityProfileSelector';
import createMovieSelector from 'Store/Selectors/createMovieSelector';

function createMovieIndexItemSelector(movieId: number) {
  return createSelector(
    createMovieSelector(movieId),
    createMovieQualityProfileSelector(movieId),
    createExecutingCommandsSelector(),
    (movie, qualityProfile, executingCommands) => {
      // If a movie is deleted this selector may fire before the parent
      // selectors, which will result in an undefined movie, if that happens
      // we want to return early here and again in the render function to avoid
      // trying to show a movie that has no information available.

      if (!movie) {
        return {};
      }

      const isRefreshingMovie = executingCommands.some((command) => {
        return (
          command.Name === REFRESH_MOVIE && command.body.movieId === movie.id
        );
      });

      const isSearchingMovie = executingCommands.some((command) => {
        return (
          command.name === MOVIE_SEARCH && command.body.movieId === movie.id
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
