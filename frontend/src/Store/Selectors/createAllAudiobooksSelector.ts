import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

function createAllAudiobooksSelector() {
  return createSelector(
    (state: AppState) => state.audiobooks,
    (audiobooks) => {
      return audiobooks.items;
    }
  );
}

export default createAllAudiobooksSelector;
