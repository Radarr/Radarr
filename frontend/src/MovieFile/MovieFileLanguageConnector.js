import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import MovieLanguage from 'Movie/MovieLanguage';
import createMovieFileSelector from 'Store/Selectors/createMovieFileSelector';

function createMapStateToProps() {
  return createSelector(
    createMovieFileSelector(),
    (movieFile) => {
      return {
        languages: movieFile ? movieFile.languages : undefined
      };
    }
  );
}

export default connect(createMapStateToProps)(MovieLanguage);
