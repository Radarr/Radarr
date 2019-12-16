import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createExistingArtistSelector from 'Store/Selectors/createExistingArtistSelector';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import AddNewArtistSearchResult from './AddNewArtistSearchResult';

function createMapStateToProps() {
  return createSelector(
    createExistingArtistSelector(),
    createDimensionsSelector(),
    (isExistingArtist, dimensions) => {
      return {
        isExistingArtist,
        isSmallScreen: dimensions.isSmallScreen
      };
    }
  );
}

export default connect(createMapStateToProps)(AddNewArtistSearchResult);
