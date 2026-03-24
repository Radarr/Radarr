import React, { useCallback } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import FilterModal from 'Components/Filter/FilterModal';
import { setMovieCollectionsFilter } from 'Store/Actions/movieCollectionActions';

interface MovieCollectionFilterModalProps {
  isOpen: boolean;
}

export default function MovieCollectionFilterModal(
  props: MovieCollectionFilterModalProps
) {
  const sectionItems = useSelector(
    (state: AppState) => state.movieCollections.items
  );
  const filterBuilderProps = useSelector(
    (state: AppState) => state.movieCollections.filterBuilderProps
  );

  const dispatch = useDispatch();

  const dispatchSetFilter = useCallback(
    (payload: { selectedFilterKey: string | number }) => {
      dispatch(setMovieCollectionsFilter(payload));
    },
    [dispatch]
  );

  return (
    <FilterModal
      {...props}
      sectionItems={sectionItems}
      filterBuilderProps={filterBuilderProps}
      customFilterType="movieCollections"
      dispatchSetFilter={dispatchSetFilter}
    />
  );
}
