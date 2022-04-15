import _ from 'lodash';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { cancelLookupMovie, lookupUnsearchedMovies } from 'Store/Actions/importMovieActions';
import ImportMovieFooter from './ImportMovieFooter';

function isMixed(items, selectedIds, defaultValue, key) {
  return _.some(items, (movie) => {
    return selectedIds.indexOf(movie.id) > -1 && movie[key] !== defaultValue;
  });
}

function createMapStateToProps() {
  return createSelector(
    (state) => state.addMovie,
    (state) => state.importMovie,
    (state, { selectedIds }) => selectedIds,
    (addMovie, importMovie, selectedIds) => {
      const {
        monitor: defaultMonitor,
        qualityProfileIds: defaultQualityProfileIds,
        minimumAvailability: defaultMinimumAvailability
      } = addMovie.defaults;

      const {
        isLookingUpMovie,
        isImporting,
        items,
        importError
      } = importMovie;

      const isMonitorMixed = isMixed(items, selectedIds, defaultMonitor, 'monitor');
      const isQualityProfileIdsMixed = isMixed(items, selectedIds, defaultQualityProfileIds, 'qualityProfileIds');
      const isMinimumAvailabilityMixed = isMixed(items, selectedIds, defaultMinimumAvailability, 'minimumAvailability');
      const hasUnsearchedItems = !isLookingUpMovie && items.some((item) => !item.isPopulated);

      return {
        selectedCount: selectedIds.length,
        isLookingUpMovie,
        isImporting,
        defaultMonitor,
        defaultQualityProfileIds,
        defaultMinimumAvailability,
        isMonitorMixed,
        isQualityProfileIdsMixed,
        isMinimumAvailabilityMixed,
        importError,
        hasUnsearchedItems
      };
    }
  );
}

const mapDispatchToProps = {
  onLookupPress: lookupUnsearchedMovies,
  onCancelLookupPress: cancelLookupMovie
};

export default connect(createMapStateToProps, mapDispatchToProps)(ImportMovieFooter);
