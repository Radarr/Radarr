import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { queueLookupMovie, setImportMovieValue } from 'Store/Actions/importMovieActions';
import createAllMoviesSelector from 'Store/Selectors/createAllMoviesSelector';
import ImportMovieTable from './ImportMovieTable';

function createMapStateToProps() {
  return createSelector(
    (state) => state.addMovie,
    (state) => state.importMovie,
    (state) => state.app.dimensions,
    createAllMoviesSelector(),
    (addMovie, importMovie, dimensions, allMovies) => {
      return {
        defaultMonitor: addMovie.defaults.monitor,
        defaultQualityProfileIds: addMovie.defaults.qualityProfileIds,
        defaultMinimumAvailability: addMovie.defaults.minimumAvailability,
        items: importMovie.items,
        isSmallScreen: dimensions.isSmallScreen,
        allMovies
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onMovieLookup(name, path) {
      dispatch(queueLookupMovie({
        name,
        path,
        term: name
      }));
    },

    onSetImportMovieValue(values) {
      dispatch(setImportMovieValue(values));
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(ImportMovieTable);
