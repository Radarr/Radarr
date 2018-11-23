import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import MovieIndexPosters from './MovieIndexPosters';

function createMapStateToProps() {
  return createSelector(
    (state) => state.movieIndex.posterOptions,
    createClientSideCollectionSelector('movies', 'movieIndex'),
    createUISettingsSelector(),
    createDimensionsSelector(),
    (posterOptions, movies, uiSettings, dimensions) => {
      return {
        posterOptions,
        showRelativeDates: uiSettings.showRelativeDates,
        shortDateFormat: uiSettings.shortDateFormat,
        timeFormat: uiSettings.timeFormat,
        isSmallScreen: dimensions.isSmallScreen,
        ...movies
      };
    }
  );
}

export default connect(createMapStateToProps)(MovieIndexPosters);
