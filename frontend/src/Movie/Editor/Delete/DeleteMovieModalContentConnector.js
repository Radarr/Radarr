import _ from 'lodash';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { bulkDeleteMovie } from 'Store/Actions/movieIndexActions';
import createAllMoviesSelector from 'Store/Selectors/createAllMoviesSelector';
import DeleteMovieModalContent from './DeleteMovieModalContent';

function createMapStateToProps() {
  return createSelector(
    (state, { movieIds }) => movieIds,
    createAllMoviesSelector(),
    (movieIds, allMovies) => {
      const selectedMovie = _.intersectionWith(allMovies, movieIds, (s, id) => {
        return s.id === id;
      });

      const sortedMovies = _.orderBy(selectedMovie, 'sortTitle');
      const movies = _.map(sortedMovies, (s) => {
        return {
          title: s.title,
          path: s.path
        };
      });

      return {
        movies
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onDeleteSelectedPress(deleteFiles, addNetImportExclusion) {
      dispatch(bulkDeleteMovie({
        movieIds: props.movieIds,
        deleteFiles,
        addNetImportExclusion
      }));

      props.onModalClose();
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(DeleteMovieModalContent);
