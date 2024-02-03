import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import DiscoverMoviePoster from './DiscoverMoviePoster';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.ui.item.movieRuntimeFormat,
    createDimensionsSelector(),
    (movieRuntimeFormat, dimensions) => {
      return {
        movieRuntimeFormat,
        isSmallScreen: dimensions.isSmallScreen
      };
    }
  );
}

export default connect(createMapStateToProps)(DiscoverMoviePoster);
