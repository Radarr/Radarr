import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createTrackFileSelector from 'Store/Selectors/createTrackFileSelector';
import EpisodeLanguage from 'Album/EpisodeLanguage';

function createMapStateToProps() {
  return createSelector(
    createTrackFileSelector(),
    (trackFile) => {
      return {
        language: trackFile ? trackFile.language : undefined
      };
    }
  );
}

export default connect(createMapStateToProps)(EpisodeLanguage);
