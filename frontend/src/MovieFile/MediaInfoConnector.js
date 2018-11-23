import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createMovieFileSelector from 'Store/Selectors/createMovieFileSelector';
import MediaInfo from './MediaInfo';

function createMapStateToProps() {
  return createSelector(
    createMovieFileSelector(),
    (episodeFile) => {
      if (episodeFile) {
        return {
          ...episodeFile.mediaInfo
        };
      }

      return {};
    }
  );
}

export default connect(createMapStateToProps)(MediaInfo);
