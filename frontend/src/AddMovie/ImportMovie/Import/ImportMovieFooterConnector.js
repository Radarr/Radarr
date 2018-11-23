import _ from 'lodash';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { cancelLookupMovie } from 'Store/Actions/importMovieActions';
import ImportMovieFooter from './ImportMovieFooter';

function isMixed(items, selectedIds, defaultValue, key) {
  return _.some(items, (series) => {
    return selectedIds.indexOf(series.id) > -1 && series[key] !== defaultValue;
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
        qualityProfileId: defaultQualityProfileId
      } = addMovie.defaults;

      const {
        isLookingUpMovie,
        isImporting,
        items
      } = importMovie;

      const isMonitorMixed = isMixed(items, selectedIds, defaultMonitor, 'monitor');
      const isQualityProfileIdMixed = isMixed(items, selectedIds, defaultQualityProfileId, 'qualityProfileId');

      return {
        selectedCount: selectedIds.length,
        isLookingUpMovie,
        isImporting,
        defaultMonitor,
        defaultQualityProfileId,
        isMonitorMixed,
        isQualityProfileIdMixed
      };
    }
  );
}

const mapDispatchToProps = {
  onCancelLookupPress: cancelLookupMovie
};

export default connect(createMapStateToProps, mapDispatchToProps)(ImportMovieFooter);
