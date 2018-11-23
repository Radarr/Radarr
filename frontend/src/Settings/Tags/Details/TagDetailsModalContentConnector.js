import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createAllMoviesSelector from 'Store/Selectors/createAllMoviesSelector';
import TagDetailsModalContent from './TagDetailsModalContent';

function findMatchingItems(ids, items) {
  return items.filter((s) => {
    return ids.includes(s.id);
  });
}

function createMatchingSeriesSelector() {
  return createSelector(
    (state, { seriesIds }) => seriesIds,
    createAllMoviesSelector(),
    findMatchingItems
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

function createMapStateToProps() {
  return createSelector(
    createMatchingSeriesSelector(),
    createMatchingDelayProfilesSelector(),
    createMatchingNotificationsSelector(),
    createMatchingRestrictionsSelector(),
    (series, delayProfiles, notifications, restrictions) => {
      return {
        series,
        delayProfiles,
        notifications,
        restrictions
      };
    }
  );
}

export default connect(createMapStateToProps)(TagDetailsModalContent);
