import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import AuthorIndexBanners from './AuthorIndexBanners';

function createMapStateToProps() {
  return createSelector(
    (state) => state.authorIndex.bannerOptions,
    createUISettingsSelector(),
    createDimensionsSelector(),
    (bannerOptions, uiSettings, dimensions) => {
      return {
        bannerOptions,
        showRelativeDates: uiSettings.showRelativeDates,
        shortDateFormat: uiSettings.shortDateFormat,
        timeFormat: uiSettings.timeFormat,
        isSmallScreen: dimensions.isSmallScreen
      };
    }
  );
}

export default connect(createMapStateToProps)(AuthorIndexBanners);
