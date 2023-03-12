import React, { useCallback } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import FilterModal from 'Components/Filter/FilterModal';
import { setMovieFilter } from 'Store/Actions/movieIndexActions';

function createMovieSelector() {
  return createSelector(
    (state) => state.movies.items,
    (movies) => {
      return movies;
    }
  );
}

function createFilterBuilderPropsSelector() {
  return createSelector(
    (state) => state.movieIndex.filterBuilderProps,
    (filterBuilderProps) => {
      return filterBuilderProps;
    }
  );
}

export default function MovieIndexFilterModal(props) {
  const sectionItems = useSelector(createMovieSelector());
  const filterBuilderProps = useSelector(createFilterBuilderPropsSelector());
  const customFilterType = 'movieIndex';

  const dispatch = useDispatch();

  const dispatchSetFilter = useCallback(
    (payload) => {
      dispatch(setMovieFilter(payload));
    },
    [dispatch]
  );

  return (
    <FilterModal
      {...props}
      sectionItems={sectionItems}
      filterBuilderProps={filterBuilderProps}
      customFilterType={customFilterType}
      dispatchSetFilter={dispatchSetFilter}
    />
  );
}
