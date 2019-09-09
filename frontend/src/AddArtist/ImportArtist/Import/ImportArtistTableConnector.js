import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { queueLookupArtist, setImportArtistValue } from 'Store/Actions/importArtistActions';
import createAllArtistSelector from 'Store/Selectors/createAllArtistSelector';
import ImportArtistTable from './ImportArtistTable';

function createMapStateToProps() {
  return createSelector(
    (state) => state.addArtist,
    (state) => state.importArtist,
    (state) => state.app.dimensions,
    createAllArtistSelector(),
    (addArtist, importArtist, dimensions, allArtists) => {
      return {
        defaultMonitor: addArtist.defaults.monitor,
        defaultQualityProfileId: addArtist.defaults.qualityProfileId,
        defaultMetadataProfileId: addArtist.defaults.metadataProfileId,
        defaultAlbumFolder: addArtist.defaults.albumFolder,
        items: importArtist.items,
        isSmallScreen: dimensions.isSmallScreen,
        allArtists
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onArtistLookup(name, path) {
      dispatch(queueLookupArtist({
        name,
        path,
        term: name
      }));
    },

    onSetImportArtistValue(values) {
      dispatch(setImportArtistValue(values));
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(ImportArtistTable);
