import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import * as commandNames from 'Commands/commandNames';
import MovieSearchCell from 'Movie/MovieSearchCell';
import { executeCommand } from 'Store/Actions/commandActions';
import createCommandsSelector from 'Store/Selectors/createCommandsSelector';
import createMovieSelector from 'Store/Selectors/createMovieSelector';
import { isCommandExecuting } from 'Utilities/Command';

function createMapStateToProps() {
  return createSelector(
    (state, { movieId }) => movieId,
    createMovieSelector(),
    createCommandsSelector(),
    (movieId, movie, commands) => {
      const isSearching = commands.some((command) => {
        const movieSearch = command.name === commandNames.MOVIE_SEARCH;

        if (!movieSearch) {
          return false;
        }

        return (
          isCommandExecuting(command) &&
          command.body.movieIds.indexOf(movieId) > -1
        );
      });

      return {
        movieMonitored: movie.monitored,
        isSearching
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onSearchPress(name, path) {
      dispatch(executeCommand({
        name: commandNames.MOVIE_SEARCH,
        movieIds: [props.movieId]
      }));
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(MovieSearchCell);
