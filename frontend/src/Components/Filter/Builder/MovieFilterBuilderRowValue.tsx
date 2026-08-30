import React, { useEffect, useMemo, useRef, useState } from 'react';
import { useSelector } from 'react-redux';
import { useDebouncedCallback } from 'use-debounce';
import Movie from 'Movie/Movie';
import createAllMoviesSelector from 'Store/Selectors/createAllMoviesSelector';
import sortByProp from 'Utilities/Array/sortByProp';
import createAjaxRequest from 'Utilities/createAjaxRequest';
import FilterBuilderRowValue from './FilterBuilderRowValue';
import FilterBuilderRowValueProps from './FilterBuilderRowValueProps';

function MovieFilterBuilderRowValue(props: FilterBuilderRowValueProps) {
  const allMovies: Movie[] = useSelector(createAllMoviesSelector());
  const [remoteMovies, setRemoteMovies] = useState<Movie[]>([]);
  const abortRequest = useRef<(() => void) | null>(null);

  const tagList = useMemo(() => {
    const movies = new Map<number, Movie>();

    [...allMovies, ...remoteMovies].forEach((movie) =>
      movies.set(movie.id, movie)
    );

    return Array.from(movies.values())
      .map((movie) => ({ id: movie.id, name: movie.title }))
      .sort(sortByProp('name'));
  }, [allMovies, remoteMovies]);

  const onQueryChange = useDebouncedCallback((term: string) => {
    abortRequest.current?.();

    if (!term.trim()) {
      setRemoteMovies([]);
      return;
    }

    const ajaxRequest = createAjaxRequest({
      url: '/movie/search',
      data: { term, limit: 20 },
    });

    abortRequest.current = ajaxRequest.abortRequest;
    ajaxRequest.request.done((movies: Movie[]) => setRemoteMovies(movies));
  }, 250);

  useEffect(() => {
    return () => {
      abortRequest.current?.();
      onQueryChange.cancel();
    };
  }, [onQueryChange]);

  return (
    <FilterBuilderRowValue
      {...props}
      tagList={tagList}
      onQueryChange={onQueryChange}
    />
  );
}

export default MovieFilterBuilderRowValue;
