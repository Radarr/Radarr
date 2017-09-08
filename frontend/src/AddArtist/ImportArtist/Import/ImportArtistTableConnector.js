import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { queueLookupSeries, setImportArtistValue } from 'Store/Actions/importArtistActions';
import createAllSeriesSelector from 'Store/Selectors/createAllSeriesSelector';
import ImportArtistTable from './ImportArtistTable';

function createMapStateToProps() {
  return createSelector(
    (state) => state.addArtist,
    (state) => state.importArtist,
    (state) => state.app.dimensions,
    createAllSeriesSelector(),
    (addArtist, importArtist, dimensions, allSeries) => {
      return {
        defaultMonitor: addArtist.defaults.monitor,
        defaultQualityProfileId: addArtist.defaults.qualityProfileId,
        defaultLanguageProfileId: addArtist.defaults.languageProfileId,
        defaultSeriesType: addArtist.defaults.seriesType,
        defaultAlbumFolder: addArtist.defaults.albumFolder,
        items: importArtist.items,
        isSmallScreen: dimensions.isSmallScreen,
        allSeries
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onSeriesLookup(name, path) {
      dispatch(queueLookupSeries({
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
