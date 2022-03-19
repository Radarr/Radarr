import { createSelector } from 'reselect';

const iCalReleaseTypes = ['all', 'cinemas', 'digital', 'physical', 'meetsMinimumAvailability'];

function createReleaseTypesSelector() {
  return createSelector(
    (state) => state.calendar.filters,
    (filters) => filters
      .filter((filter) => iCalReleaseTypes.includes(filter.key))
      .map((filter) => ({
        key: filter.key,
        value: filter.label,
        unselectValues: filter.unselectFilters
      }))
  );
}

export default createReleaseTypesSelector;
