import _ from 'lodash';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { cancelLookupMovie } from 'Store/Actions/importMovieActions';
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
        qualityProfileId: defaultQualityProfileId,
        minimumAvailability: defaultMinimumAvailability
      } = addMovie.defaults;

      const {
        isLookingUpMovie,
        isImporting,
        items
      } = importMovie;

      const isMonitorMixed = isMixed(items, selectedIds, defaultMonitor, 'monitor');
      const isQualityProfileIdMixed = isMixed(items, selectedIds, defaultQualityProfileId, 'qualityProfileId');
      const isMinimumAvailabilityMixed = isMixed(items, selectedIds, defaultMinimumAvailability, 'minimumAvailability');

      return {
        selectedCount: selectedIds.length,
        isLookingUpMovie,
        isImporting,
        defaultMonitor,
        defaultQualityProfileId,
        defaultMinimumAvailability,
        isMonitorMixed,
        isQualityProfileIdMixed,
        isMinimumAvailabilityMixed
      };
    }
  );
}

const mapDispatchToProps = {
  onCancelLookupPress: cancelLookupMovie
};

export default connect(createMapStateToProps, mapDispatchToProps)(ImportMovieFooter);
