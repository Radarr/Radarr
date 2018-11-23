import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import MovieIndexOverviews from './MovieIndexOverviews';

function createMapStateToProps() {
  return createSelector(
    (state) => state.movieIndex.overviewOptions,
    createClientSideCollectionSelector('movies', 'movieIndex'),
    createUISettingsSelector(),
    createDimensionsSelector(),
    (overviewOptions, movies, uiSettings, dimensions) => {
      return {
        overviewOptions,
        showRelativeDates: uiSettings.showRelativeDates,
        shortDateFormat: uiSettings.shortDateFormat,
        longDateFormat: uiSettings.longDateFormat,
        timeFormat: uiSettings.timeFormat,
        isSmallScreen: dimensions.isSmallScreen,
        ...movies
      };
    }
  );
}

export default connect(createMapStateToProps)(MovieIndexOverviews);
