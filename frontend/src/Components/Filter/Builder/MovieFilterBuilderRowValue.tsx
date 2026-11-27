import React from 'react';
import { useSelector } from 'react-redux';
import Movie from 'Movie/Movie';
import createAllMoviesSelector from 'Store/Selectors/createAllMoviesSelector';
import sortByProp from 'Utilities/Array/sortByProp';
import FilterBuilderRowValue, {
  FilterBuilderRowValueProps,
} from './FilterBuilderRowValue';

type MovieFilterBuilderRowValueProps<T> = Omit<
  FilterBuilderRowValueProps<T, number>,
  'taglist'
>;
function MovieFilterBuilderRowValue<T>(
  props: MovieFilterBuilderRowValueProps<T>
) {
  const allMovies: Movie[] = useSelector(createAllMoviesSelector());

  const tagList = allMovies
    .map((movie) => ({ id: movie.id, name: movie.title }))
    .sort(sortByProp('name'));

  return <FilterBuilderRowValue {...props} tagList={tagList} />;
}

export default MovieFilterBuilderRowValue;
