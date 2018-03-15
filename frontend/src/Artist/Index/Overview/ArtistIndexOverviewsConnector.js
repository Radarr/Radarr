import { createSelector } from 'reselect';
import connectSection from 'Store/connectSection';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import ArtistIndexOverviews from './ArtistIndexOverviews';

function createMapStateToProps() {
  return createSelector(
    (state) => state.artistIndex.overviewOptions,
    createClientSideCollectionSelector(),
    createUISettingsSelector(),
    createDimensionsSelector(),
    (overviewOptions, artist, uiSettings, dimensions) => {
      return {
        overviewOptions,
        showRelativeDates: uiSettings.showRelativeDates,
        shortDateFormat: uiSettings.shortDateFormat,
        timeFormat: uiSettings.timeFormat,
        isSmallScreen: dimensions.isSmallScreen,
        ...artist
      };
    }
  );
}

export default connectSection(
  createMapStateToProps,
  undefined,
  undefined,
  undefined,
  { section: 'artist', uiSection: 'artistIndex' }
)(ArtistIndexOverviews);
