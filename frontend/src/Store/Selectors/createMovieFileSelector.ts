import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

function createMovieFileSelector() {
  return createSelector(
    (_: AppState, { movieFileId }: { movieFileId: number }) => movieFileId,
    (state: AppState) => state.movieFiles,
    (movieFileId, movieFiles) => {
      if (!movieFileId) {
        return;
      }

      return movieFiles.items.find((movieFile) => movieFile.id === movieFileId);
    }
  );
}

export default createMovieFileSelector;
