import React from 'react';
import MovieLanguages from 'Movie/MovieLanguages';
import useMovieFile from './useMovieFile';

interface MovieFileLanguagesProps {
  movieFileId: number;
}

function MovieFileLanguages({ movieFileId }: MovieFileLanguagesProps) {
  const movieFile = useMovieFile(movieFileId);

  return <MovieLanguages languages={movieFile?.languages ?? []} />;
}

export default MovieFileLanguages;
