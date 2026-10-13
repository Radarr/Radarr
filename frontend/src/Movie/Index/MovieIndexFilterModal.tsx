import React, { useCallback } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import FilterModal, { FilterModalProps } from 'Components/Filter/FilterModal';
import Movie from 'Movie/Movie';
import { setMovieFilter } from 'Store/Actions/movieIndexActions';

function createMovieSelector() {
  return createSelector(
    (state: AppState) => state.movies.items,
    (movies) => {
      return movies;
    }
  );
}

function createFilterBuilderPropsSelector() {
  return createSelector(
    (state: AppState) => state.movieIndex.filterBuilderProps,
    (filterBuilderProps) => {
      return filterBuilderProps;
    }
  );
}

type MovieIndexFilterModalProps = FilterModalProps<Movie>;

export default function MovieIndexFilterModal(
  props: MovieIndexFilterModalProps
) {
  const sectionItems = useSelector(createMovieSelector());
  const filterBuilderProps = useSelector(createFilterBuilderPropsSelector());
  const customFilterType = 'movieIndex';

  const dispatch = useDispatch();

  const dispatchSetFilter = useCallback(
    (payload: unknown) => {
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
