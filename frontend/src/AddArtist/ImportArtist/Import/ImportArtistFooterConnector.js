import _ from 'lodash';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import ImportArtistFooter from './ImportArtistFooter';

function isMixed(items, selectedIds, defaultValue, key) {
  return _.some(items, (artist) => {
    return selectedIds.indexOf(artist.id) > -1 && artist[key] !== defaultValue;
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
        metadataProfileId: defaultMetadataProfileId,
        albumFolder: defaultAlbumFolder
      } = addArtist.defaults;

      const items = importArtist.items;

      const isLookingUpArtist = _.some(importArtist.items, (artist) => {
        return !artist.isPopulated && artist.error == null;
      });

      const isMonitorMixed = isMixed(items, selectedIds, defaultMonitor, 'monitor');
      const isQualityProfileIdMixed = isMixed(items, selectedIds, defaultQualityProfileId, 'qualityProfileId');
      const isLanguageProfileIdMixed = isMixed(items, selectedIds, defaultLanguageProfileId, 'languageProfileId');
      const isMetadataProfileIdMixed = isMixed(items, selectedIds, defaultMetadataProfileId, 'metadataProfileId');
      const isAlbumFolderMixed = isMixed(items, selectedIds, defaultAlbumFolder, 'albumFolder');

      return {
        selectedCount: selectedIds.length,
        isImporting: importArtist.isImporting,
        isLookingUpArtist,
        defaultMonitor,
        defaultQualityProfileId,
        defaultLanguageProfileId,
        defaultMetadataProfileId,
        defaultAlbumFolder,
        isMonitorMixed,
        isQualityProfileIdMixed,
        isLanguageProfileIdMixed,
        isMetadataProfileIdMixed,
        isAlbumFolderMixed
      };
    }
  );
}

export default connect(createMapStateToProps)(ImportArtistFooter);
