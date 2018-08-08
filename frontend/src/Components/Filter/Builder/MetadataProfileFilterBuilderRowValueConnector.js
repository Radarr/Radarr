import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import FilterBuilderRowValue from './FilterBuilderRowValue';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.metadataProfiles,
    (metadataProfiles) => {
      const tagList = metadataProfiles.items.map((metadataProfile) => {
        const {
          id,
          name
        } = metadataProfile;

        return {
          id,
          name
        };
      });

      return {
        tagList
      };
    }
  );
}

export default connect(createMapStateToProps)(FilterBuilderRowValue);
