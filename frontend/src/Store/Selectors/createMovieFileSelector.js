import { createSelector } from 'reselect';

function createMovieFileSelector() {
  return createSelector(
    (state, { movieFileId }) => movieFileId,
    (state) => state.movieFiles,
    (movieFileId, movieFiles) => {
      if (!movieFileId) {
        return;
      }

      return movieFiles.items.find((movieFile) => movieFile.id === movieFileId);
    }
  );
}

export default createMovieFileSelector;
