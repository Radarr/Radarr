import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { queueLookupMovie, setImportMovieValue } from 'Store/Actions/importMovieActions';
import ImportMovieTable from './ImportMovieTable';

function createMapStateToProps() {
  return createSelector(
    (state) => state.addMovie,
    (state) => state.importMovie,
    (state) => state.app.dimensions,
    (addMovie, importMovie, dimensions) => {
      return {
        defaultMonitor: addMovie.defaults.monitor,
        defaultQualityProfileId: addMovie.defaults.qualityProfileId,
        defaultMinimumAvailability: addMovie.defaults.minimumAvailability,
        items: importMovie.items,
        isSmallScreen: dimensions.isSmallScreen
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onMovieLookup(name, path, relativePath) {
      dispatch(queueLookupMovie({
        name,
        path,
        relativePath,
        term: name
      }));
    },

    onSetImportMovieValue(values) {
      dispatch(setImportMovieValue(values));
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(ImportMovieTable);
