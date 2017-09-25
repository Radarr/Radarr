import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createTrackFileSelector from 'Store/Selectors/createTrackFileSelector';
import MediaInfo from './MediaInfo';

function createMapStateToProps() {
  return createSelector(
    createTrackFileSelector(),
    (trackFile) => {
      if (trackFile) {
        return {
          ...trackFile.mediaInfo
        };
      }

      return {};
    }
  );
}

export default connect(createMapStateToProps)(MediaInfo);
