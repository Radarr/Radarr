import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import MovieLanguages from 'Movie/MovieLanguages';
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

export default connect(createMapStateToProps)(MovieLanguages);
