import _ from 'lodash';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createAllMoviesSelector from 'Store/Selectors/createAllMoviesSelector';
import { bulkDeleteMovie } from 'Store/Actions/movieEditorActions';
import DeleteMovieModalContent from './DeleteMovieModalContent';

function createMapStateToProps() {
  return createSelector(
    (state, { seriesIds }) => seriesIds,
    createAllMoviesSelector(),
    (seriesIds, allMovies) => {
      const selectedMovie = _.intersectionWith(allMovies, seriesIds, (s, id) => {
        return s.id === id;
      });

      const sortedSeries = _.orderBy(selectedMovie, 'sortTitle');
      const series = _.map(sortedSeries, (s) => {
        return {
          title: s.title,
          path: s.path
        };
      });

      return {
        series
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onDeleteSelectedPress(deleteFiles) {
      dispatch(bulkDeleteMovie({
        seriesIds: props.seriesIds,
        deleteFiles
      }));

      props.onModalClose();
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(DeleteMovieModalContent);
