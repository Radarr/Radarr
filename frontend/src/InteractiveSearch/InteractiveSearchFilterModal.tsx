import React, { useCallback } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import FilterModal, { FilterModalProps } from 'Components/Filter/FilterModal';
import { setReleasesFilter } from 'Store/Actions/releaseActions';
import Release from 'typings/Release';

function createReleasesSelector() {
  return createSelector(
    (state: AppState) => state.releases.items,
    (releases) => {
      return releases;
    }
  );
}

function createFilterBuilderPropsSelector() {
  return createSelector(
    (state: AppState) => state.releases.filterBuilderProps,
    (filterBuilderProps) => {
      return filterBuilderProps;
    }
  );
}

type InteractiveSearchFilterModalProps = FilterModalProps<Release>;

export default function InteractiveSearchFilterModal({
  ...otherProps
}: InteractiveSearchFilterModalProps) {
  const sectionItems = useSelector(createReleasesSelector());
  const filterBuilderProps = useSelector(createFilterBuilderPropsSelector());

  const dispatch = useDispatch();

  const dispatchSetFilter = useCallback(
    (payload: unknown) => {
      dispatch(setReleasesFilter(payload));
    },
    [dispatch]
  );

  return (
    <FilterModal
      {...otherProps}
      sectionItems={sectionItems}
      filterBuilderProps={filterBuilderProps}
      customFilterType="releases"
      dispatchSetFilter={dispatchSetFilter}
    />
  );
}
