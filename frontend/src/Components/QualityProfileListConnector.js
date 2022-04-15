import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import QualityProfileList from './QualityProfileList';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.qualityProfiles.items,
    (qualityProfileList) => {
      return {
        qualityProfileList
      };
    }
  );
}

export default connect(createMapStateToProps)(QualityProfileList);
