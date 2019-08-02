import _ from 'lodash';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import ImportArtistFooter from './ImportArtistFooter';
import { lookupUnsearchedArtist, cancelLookupArtist } from 'Store/Actions/importArtistActions';

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
        metadataProfileId: defaultMetadataProfileId,
        albumFolder: defaultAlbumFolder
      } = addArtist.defaults;

      const {
        isLookingUpArtist,
        isImporting,
        items
      } = importArtist;

      const isMonitorMixed = isMixed(items, selectedIds, defaultMonitor, 'monitor');
      const isQualityProfileIdMixed = isMixed(items, selectedIds, defaultQualityProfileId, 'qualityProfileId');
      const isMetadataProfileIdMixed = isMixed(items, selectedIds, defaultMetadataProfileId, 'metadataProfileId');
      const isAlbumFolderMixed = isMixed(items, selectedIds, defaultAlbumFolder, 'albumFolder');
      const hasUnsearchedItems = !isLookingUpArtist && items.some((item) => !item.isPopulated);

      return {
        selectedCount: selectedIds.length,
        isLookingUpArtist,
        isImporting,
        defaultMonitor,
        defaultQualityProfileId,
        defaultMetadataProfileId,
        defaultAlbumFolder,
        isMonitorMixed,
        isQualityProfileIdMixed,
        isMetadataProfileIdMixed,
        isAlbumFolderMixed,
        hasUnsearchedItems
      };
    }
  );
}

const mapDispatchToProps = {
  onLookupPress: lookupUnsearchedArtist,
  onCancelLookupPress: cancelLookupArtist
};

export default connect(createMapStateToProps, mapDispatchToProps)(ImportArtistFooter);
