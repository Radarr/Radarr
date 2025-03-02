import React, { useCallback } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import FilterModal from 'Components/Filter/FilterModal';
import { setReleasesFilter } from 'Store/Actions/releaseActions';

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

interface InteractiveSearchFilterModalProps {
  isOpen: boolean;
}

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
      // TODO: Don't spread all the props
      {...otherProps}
      sectionItems={sectionItems}
      filterBuilderProps={filterBuilderProps}
      customFilterType="releases"
      dispatchSetFilter={dispatchSetFilter}
    />
  );
}
