import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createAllAuthorSelector from 'Store/Selectors/createAllAuthorsSelector';
import TagDetailsModalContent from './TagDetailsModalContent';

function findMatchingItems(ids, items) {
  return items.filter((s) => {
    return ids.includes(s.id);
  });
}

function createUnorderedMatchingAuthorSelector() {
  return createSelector(
    (state, { authorIds }) => authorIds,
    createAllAuthorSelector(),
    findMatchingItems
  );
}

function createMatchingAuthorSelector() {
  return createSelector(
    createUnorderedMatchingAuthorSelector(),
    (authors) => {
      return authors.sort((authorA, authorB) => {
        const sortNameA = authorA.sortName;
        const sortNameB = authorB.sortName;

        if (sortNameA > sortNameB) {
          return 1;
        } else if (sortNameA < sortNameB) {
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

function createMatchingImportListsSelector() {
  return createSelector(
    (state, { importListIds }) => importListIds,
    (state) => state.settings.importLists.items,
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

function createMatchingReleaseProfilesSelector() {
  return createSelector(
    (state, { restrictionIds }) => restrictionIds,
    (state) => state.settings.releaseProfiles.items,
    findMatchingItems
  );
}

function createMapStateToProps() {
  return createSelector(
    createMatchingAuthorSelector(),
    createMatchingDelayProfilesSelector(),
    createMatchingImportListsSelector(),
    createMatchingNotificationsSelector(),
    createMatchingReleaseProfilesSelector(),
    (author, delayProfiles, importLists, notifications, releaseProfiles) => {
      return {
        author,
        delayProfiles,
        importLists,
        notifications,
        releaseProfiles
      };
    }
  );
}

export default connect(createMapStateToProps)(TagDetailsModalContent);
