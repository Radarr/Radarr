import React, { useCallback } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { createSelector } from 'reselect';
import AppState from 'App/State/AppState';
import FilterModal from 'Components/Filter/FilterModal';
import Movie from 'Movie/Movie';
import { setMovieFilter } from 'Store/Actions/movieIndexActions';

function createMovieSelector() {
  return createSelector(
    (state: AppState) => state.movies.items,
    (state: AppState) => state.movies.facets,
    (movies, facets) => {
      const facetMovies = [
        ...(facets.certifications ?? []).map((certification) => ({
          certification,
        })),
        ...(facets.collections ?? []).map((title) => ({
          collection: { title },
        })),
        ...(facets.genres ?? []).map((genre) => ({ genres: [genre] })),
        ...(facets.keywords ?? []).map((keyword) => ({ keywords: [keyword] })),
        ...(facets.originalLanguages ?? []).map((name) => ({
          originalLanguage: { name },
        })),
        ...(facets.releaseGroups ?? []).map((releaseGroup) => ({
          statistics: { releaseGroups: [releaseGroup] },
        })),
        ...(facets.studios ?? []).map((studio) => ({ studio })),
      ] as Movie[];

      return [...movies, ...facetMovies];
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

interface MovieIndexFilterModalProps {
  isOpen: boolean;
}

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
      // TODO: Don't spread all the props
      {...props}
      sectionItems={sectionItems}
      filterBuilderProps={filterBuilderProps}
      customFilterType={customFilterType}
      dispatchSetFilter={dispatchSetFilter}
    />
  );
}
