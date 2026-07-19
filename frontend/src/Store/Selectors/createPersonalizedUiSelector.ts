import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';

export default function createPersonalizedUiSelector() {
  return createSelector(
    (state: AppState) => state.personalizedUi,
    (personalizedUi) => personalizedUi
  );
}
