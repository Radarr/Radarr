import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createMovieFileSelector from 'Store/Selectors/createMovieFileSelector';
import MediaInfo from './MediaInfo';

function createMapStateToProps() {
  return createSelector(
    createMovieFileSelector(),
    (movieFile) => {
      if (movieFile) {
        return {
          ...movieFile.mediaInfo
        };
      }

      return {};
    }
  );
}

export default connect(createMapStateToProps)(MediaInfo);
