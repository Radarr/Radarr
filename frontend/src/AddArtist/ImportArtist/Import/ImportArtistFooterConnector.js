import _ from 'lodash';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import ImportArtistFooter from './ImportArtistFooter';

function isMixed(items, selectedIds, defaultValue, key) {
  return _.some(items, (series) => {
    return selectedIds.indexOf(series.id) > -1 && series[key] !== defaultValue;
  });
}

function createMapStateToProps() {
  return createSelector(
    (state) => state.addArtist,
    (state) => state.importArtist,
    (state, { selectedIds }) => selectedIds,
    (addArtist, importArtist, selectedIds) => {
      const {
        monitor: defaultMonitor,
        qualityProfileId: defaultQualityProfileId,
        languageProfileId: defaultLanguageProfileId,
        seriesType: defaultSeriesType,
        albumFolder: defaultAlbumFolder
      } = addArtist.defaults;

      const items = importArtist.items;

      const isLookingUpArtist = _.some(importArtist.items, (series) => {
        return !series.isPopulated && series.error == null;
      });

      const isMonitorMixed = isMixed(items, selectedIds, defaultMonitor, 'monitor');
      const isQualityProfileIdMixed = isMixed(items, selectedIds, defaultQualityProfileId, 'qualityProfileId');
      const isLanguageProfileIdMixed = isMixed(items, selectedIds, defaultLanguageProfileId, 'languageProfileId');
      const isSeriesTypeMixed = isMixed(items, selectedIds, defaultSeriesType, 'seriesType');
      const isAlbumFolderMixed = isMixed(items, selectedIds, defaultAlbumFolder, 'albumFolder');

      return {
        selectedCount: selectedIds.length,
        isImporting: importArtist.isImporting,
        isLookingUpArtist,
        defaultMonitor,
        defaultQualityProfileId,
        defaultLanguageProfileId,
        defaultSeriesType,
        defaultAlbumFolder,
        isMonitorMixed,
        isQualityProfileIdMixed,
        isLanguageProfileIdMixed,
        isSeriesTypeMixed,
        isAlbumFolderMixed
      };
    }
  );
}

export default connect(createMapStateToProps)(ImportArtistFooter);
