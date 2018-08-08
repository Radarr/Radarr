import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import ArtistIndexBanners from './ArtistIndexBanners';

function createMapStateToProps() {
  return createSelector(
    (state) => state.artistIndex.bannerOptions,
    createClientSideCollectionSelector('artist', 'artistIndex'),
    createUISettingsSelector(),
    createDimensionsSelector(),
    (bannerOptions, artist, uiSettings, dimensions) => {
      return {
        bannerOptions,
        showRelativeDates: uiSettings.showRelativeDates,
        shortDateFormat: uiSettings.shortDateFormat,
        timeFormat: uiSettings.timeFormat,
        isSmallScreen: dimensions.isSmallScreen,
        ...artist
      };
    }
  );
}

export default connect(createMapStateToProps)(ArtistIndexBanners);
