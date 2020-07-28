import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import MovieIndexOverviews from './MovieIndexOverviews';

function createMapStateToProps() {
  return createSelector(
    (state) => state.movieIndex.overviewOptions,
    createUISettingsSelector(),
    createDimensionsSelector(),
    (overviewOptions, uiSettings, dimensions) => {
      return {
        overviewOptions,
        showRelativeDates: uiSettings.showRelativeDates,
        shortDateFormat: uiSettings.shortDateFormat,
        longDateFormat: uiSettings.longDateFormat,
        timeFormat: uiSettings.timeFormat,
        isSmallScreen: dimensions.isSmallScreen
      };
    }
  );
}

export default connect(createMapStateToProps)(MovieIndexOverviews);
