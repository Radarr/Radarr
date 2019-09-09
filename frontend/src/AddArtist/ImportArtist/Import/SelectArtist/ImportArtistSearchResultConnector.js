import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createExistingArtistSelector from 'Store/Selectors/createExistingArtistSelector';
import ImportArtistSearchResult from './ImportArtistSearchResult';

function createMapStateToProps() {
  return createSelector(
    createExistingArtistSelector(),
    (isExistingArtist) => {
      return {
        isExistingArtist
      };
    }
  );
}

export default connect(createMapStateToProps)(ImportArtistSearchResult);
