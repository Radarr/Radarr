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
        defaultLanguageProfileId: addArtist.defaults.languageProfileId,
        defaultSeriesType: addArtist.defaults.seriesType,
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
    onSeriesLookup(name, path) {
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
