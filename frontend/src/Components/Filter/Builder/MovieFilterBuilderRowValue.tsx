import React from 'react';
import { useSelector } from 'react-redux';
import Movie from 'Movie/Movie';
import createAllMoviesSelector from 'Store/Selectors/createAllMoviesSelector';
import FilterBuilderRowValue from './FilterBuilderRowValue';
import FilterBuilderRowValueProps from './FilterBuilderRowValueProps';

function MovieFilterBuilderRowValue(props: FilterBuilderRowValueProps) {
  const allMovies: Movie[] = useSelector(createAllMoviesSelector());

  const tagList = allMovies.map((movie) => {
    return {
      id: movie.id,
      name: movie.title,
    };
  });

  return <FilterBuilderRowValue {...props} tagList={tagList} />;
}

export default MovieFilterBuilderRowValue;
