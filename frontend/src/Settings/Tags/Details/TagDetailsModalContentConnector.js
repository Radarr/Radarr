import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createAllMoviesSelector from 'Store/Selectors/createAllMoviesSelector';
import TagDetailsModalContent from './TagDetailsModalContent';

function findMatchingItems(ids, items) {
  return items.filter((s) => {
    return ids.includes(s.id);
  });
}

function createUnorderedMatchingMoviesSelector() {
  return createSelector(
    (state, { movieIds }) => movieIds,
    createAllMoviesSelector(),
    findMatchingItems
  );
}

function createMatchingMoviesSelector() {
  return createSelector(
    createUnorderedMatchingMoviesSelector(),
    (movies) => {
      return movies.sort((movieA, movieB) => {
        const sortTitleA = movieA.sortTitle;
        const sortTitleB = movieB.sortTitle;

        if (sortTitleA > sortTitleB) {
          return 1;
        } else if (sortTitleA < sortTitleB) {
          return -1;
        }

        return 0;
      });
    }
  );
}

function createMatchingDelayProfilesSelector() {
  return createSelector(
    (state, { delayProfileIds }) => delayProfileIds,
    (state) => state.settings.delayProfiles.items,
    findMatchingItems
  );
}

function createMatchingNotificationsSelector() {
  return createSelector(
    (state, { notificationIds }) => notificationIds,
    (state) => state.settings.notifications.items,
    findMatchingItems
  );
}

function createMatchingRestrictionsSelector() {
  return createSelector(
    (state, { restrictionIds }) => restrictionIds,
    (state) => state.settings.restrictions.items,
    findMatchingItems
  );
}

function createMatchingImportListsSelector() {
  return createSelector(
    (state, { importListIds }) => importListIds,
    (state) => state.settings.importLists.items,
    findMatchingItems
  );
}

function createMatchingIndexersSelector() {
  return createSelector(
    (state, { indexerIds }) => indexerIds,
    (state) => state.settings.indexers.items,
    findMatchingItems
  );
}

function createMapStateToProps() {
  return createSelector(
    createMatchingMoviesSelector(),
    createMatchingDelayProfilesSelector(),
    createMatchingNotificationsSelector(),
    createMatchingRestrictionsSelector(),
    createMatchingImportListsSelector(),
    createMatchingIndexersSelector(),
    (movies, delayProfiles, notifications, restrictions, importLists, indexers) => {
      return {
        movies,
        delayProfiles,
        notifications,
        restrictions,
        importLists,
        indexers
      };
    }
  );
}

export default connect(createMapStateToProps)(TagDetailsModalContent);
