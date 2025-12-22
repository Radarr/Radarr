import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

function createAddAudiobookSelector() {
  return createSelector(
    (state: AppState) => state.addAudiobook,
    (addAudiobook) => {
      return addAudiobook;
    }
  );
}

export default createAddAudiobookSelector;
