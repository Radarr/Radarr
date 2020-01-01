import _ from 'lodash';
import { handleActions } from 'redux-actions';
import getSectionState from 'Utilities/State/getSectionState';
import updateSectionState from 'Utilities/State/updateSectionState';
import {
  SET,
  UPDATE,
  UPDATE_ITEM,
  UPDATE_SERVER_SIDE_COLLECTION,
  CLEAR_PENDING_CHANGES,
  REMOVE_ITEM
} from 'Store/Actions/baseActions';

const blacklistedProperties = [
  'section',
  'id'
];

export default function createHandleActions(handlers, defaultState, section) {
  return handleActions({

    [SET]: function(state, { payload }) {
      const payloadSection = payload.section;
      const [baseSection] = payloadSection.split('.');

      if (section === baseSection) {
        const newState = Object.assign(getSectionState(state, payloadSection),
          _.omit(payload, blacklistedProperties));

        return updateSectionState(state, payloadSection, newState);
      }

      return state;
    },

    [UPDATE]: function(state, { payload }) {
      const payloadSection = payload.section;
      const [baseSection] = payloadSection.split('.');

      if (section === baseSection) {
        const newState = getSectionState(state, payloadSection);

        if (_.isArray(payload.data)) {
          newState.items = payload.data;
          newState.itemMap = _.zipObject(_.map(payload.data, 'id'), _.range(payload.data.length));
        } else {
          newState.item = payload.data;
        }

        return updateSectionState(state, payloadSection, newState);
      }

      return state;
    },

    [UPDATE_ITEM]: function(state, { payload }) {
      const {
        section: payloadSection,
        updateOnly = false,
        ...otherProps
      } = payload;

      const [baseSection] = payloadSection.split('.');

      if (section === baseSection) {
        const newState = getSectionState(state, payloadSection);
        const items = newState.items;

        if (!newState.itemMap) {
          newState.itemMap = _.zipObject(_.map(items, 'id'), _.range(items.length));
        }

        const index = payload.id in newState.itemMap ? newState.itemMap[payload.id] : -1;

        newState.items = [...items];

        // TODO: Move adding to it's own reducer
        if (index >= 0) {
          const item = items[index];
          const newItem = { ...item, ...otherProps };

          // if the item to update is equal to existing, then don't actually update
          // to prevent costly reselections
          if (_.isEqual(item, newItem)) {
            return state;
          }

          newState.items.splice(index, 1, newItem);
        } else if (!updateOnly) {
          const newIndex = newState.items.push({ ...otherProps }) - 1;

          newState.itemMap[payload.id] = newIndex;
        }

        return updateSectionState(state, payloadSection, newState);
      }

      return state;
    },

    [CLEAR_PENDING_CHANGES]: function(state, { payload }) {
      const payloadSection = payload.section;
      const [baseSection] = payloadSection.split('.');

      if (section === baseSection) {
        const newState = getSectionState(state, payloadSection);
        newState.pendingChanges = {};

        if (newState.hasOwnProperty('saveError')) {
          newState.saveError = null;
        }

        return updateSectionState(state, payloadSection, newState);
      }

      return state;
    },

    [REMOVE_ITEM]: function(state, { payload }) {
      const payloadSection = payload.section;
      const [baseSection] = payloadSection.split('.');

      if (section === baseSection) {
        const newState = getSectionState(state, payloadSection);

        newState.items = [...newState.items];
        _.remove(newState.items, { id: payload.id });

        newState.itemMap = _.zipObject(_.map(newState.items, 'id'), _.range(newState.items.length));

        return updateSectionState(state, payloadSection, newState);
      }

      return state;
    },

    [UPDATE_SERVER_SIDE_COLLECTION]: function(state, { payload }) {
      const payloadSection = payload.section;
      const [baseSection] = payloadSection.split('.');

      if (section === baseSection) {
        const data = payload.data;
        const newState = getSectionState(state, payloadSection);

        const serverState = _.omit(data, ['records']);
        const calculatedState = {
          totalPages: Math.max(Math.ceil(data.totalRecords / data.pageSize), 1),
          items: data.records
        };

        return updateSectionState(state, payloadSection, Object.assign(newState, serverState, calculatedState));
      }

      return state;
    },

    ...handlers

  }, defaultState);
}
