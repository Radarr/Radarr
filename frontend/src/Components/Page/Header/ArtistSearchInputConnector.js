import _ from 'lodash';
import { connect } from 'react-redux';
import { push } from 'react-router-redux';
import { createSelector } from 'reselect';
import createAllArtistSelector from 'Store/Selectors/createAllArtistSelector';
import ArtistSearchInput from './ArtistSearchInput';

function createMapStateToProps() {
  return createSelector(
    createAllArtistSelector(),
    (series) => {
      return {
        series: _.sortBy(series, 'sortName')
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onGoToSeries(nameSlug) {
      dispatch(push(`${window.Sonarr.urlBase}/artist/${nameSlug}`));
    },

    onGoToAddNewArtist(query) {
      dispatch(push(`${window.Sonarr.urlBase}/add/new?term=${encodeURIComponent(query)}`));
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(ArtistSearchInput);
