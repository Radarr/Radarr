import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createBookFileSelector from 'Store/Selectors/createBookFileSelector';
import MediaInfo from './MediaInfo';

function createMapStateToProps() {
  return createSelector(
    createBookFileSelector(),
    (bookFile) => {
      if (bookFile) {
        return {
          ...bookFile.mediaInfo
        };
      }

      return {};
    }
  );
}

export default connect(createMapStateToProps)(MediaInfo);
