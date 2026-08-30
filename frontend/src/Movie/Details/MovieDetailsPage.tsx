import React, { useEffect } from 'react';
import { useDispatch, useSelector } from 'react-redux';
import { useHistory, useParams } from 'react-router';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import NotFound from 'Components/NotFound';
import useApiQuery from 'Helpers/Hooks/useApiQuery';
import usePrevious from 'Helpers/Hooks/usePrevious';
import Movie from 'Movie/Movie';
import { updateItem } from 'Store/Actions/baseActions';
import createAllMoviesSelector from 'Store/Selectors/createAllMoviesSelector';
import translate from 'Utilities/String/translate';
import MovieDetails from './MovieDetails';

interface MovieLink {
  title: string;
  titleSlug: string;
}

interface MovieDetailsResponse {
  movie: Movie;
  previousMovie: MovieLink;
  nextMovie: MovieLink;
}

function MovieDetailsPage() {
  const allMovies = useSelector(createAllMoviesSelector());
  const { titleSlug } = useParams<{ titleSlug: string }>();
  const history = useHistory();
  const dispatch = useDispatch();
  const { data, isLoading, isError } = useApiQuery<MovieDetailsResponse>({
    path: `/movie/slug/${encodeURIComponent(titleSlug)}`,
  });

  const movieIndex = allMovies.findIndex(
    (movie) => movie.titleSlug === titleSlug
  );

  const previousIndex = usePrevious(movieIndex);

  useEffect(() => {
    if (data) {
      dispatch(updateItem({ section: 'movies', ...data.movie }));
    }
  }, [data, dispatch]);

  useEffect(() => {
    if (
      isError &&
      movieIndex === -1 &&
      previousIndex !== -1 &&
      previousIndex !== undefined
    ) {
      history.push(`${window.Radarr.urlBase}/`);
    }
  }, [isError, movieIndex, previousIndex, history]);

  if (isError && movieIndex === -1) {
    return <NotFound message={translate('MovieCannotBeFound')} />;
  }

  if (isLoading || movieIndex === -1) {
    return <LoadingIndicator />;
  }

  return (
    <MovieDetails
      movieId={allMovies[movieIndex].id}
      previousMovie={data?.previousMovie}
      nextMovie={data?.nextMovie}
    />
  );
}

export default MovieDetailsPage;
