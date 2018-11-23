import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createMovieFileSelector from 'Store/Selectors/createMovieFileSelector';
import EpisodeLanguage from 'Episode/EpisodeLanguage';

function createMapStateToProps() {
  return createSelector(
    createMovieFileSelector(),
    (episodeFile) => {
      return {
        language: episodeFile ? episodeFile.language : undefined
      };
    }
  );
}

export default connect(createMapStateToProps)(EpisodeLanguage);
