import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import DiscoverMoviePoster from './DiscoverMoviePoster';

function createMapStateToProps() {
  return createSelector(
    createDimensionsSelector(),
    ( dimensions) => {
      return {
        isSmallScreen: dimensions.isSmallScreen
      };
    }
  );
}

export default connect(createMapStateToProps)(DiscoverMoviePoster);
