import EpisodeLanguage from 'Episode/EpisodeLanguage';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createMovieFileSelector from 'Store/Selectors/createMovieFileSelector';

function createMapStateToProps() {
  return createSelector(
    createMovieFileSelector(),
    (movieFile) => {
      return {
        language: movieFile ? movieFile.language : undefined
      };
    }
  );
}

export default connect(createMapStateToProps)(EpisodeLanguage);
