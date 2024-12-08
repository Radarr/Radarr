import { useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

function createMovieFileSelector(movieFileId?: number) {
  return createSelector(
    (state: AppState) => state.movieFiles.items,
    (movieFiles) => {
      return movieFiles.find(({ id }) => id === movieFileId);
    }
  );
}

function useMovieFile(movieFileId: number | undefined) {
  return useSelector(createMovieFileSelector(movieFileId));
}

export default useMovieFile;
