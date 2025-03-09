import React, { useEffect } from 'react';
import { useSelector } from 'react-redux';
import { useParams } from 'react-router';
import { useHistory } from 'react-router-dom';
import NotFound from 'Components/NotFound';
import usePrevious from 'Helpers/Hooks/usePrevious';
import createAllMoviesSelector from 'Store/Selectors/createAllMoviesSelector';
import translate from 'Utilities/String/translate';
import MovieDetails from './MovieDetails';

function MovieDetailsPage() {
  const allMovies = useSelector(createAllMoviesSelector());
  const { titleSlug } = useParams<{ titleSlug: string }>();
  const history = useHistory();

  const movieIndex = allMovies.findIndex(
    (movie) => movie.titleSlug === titleSlug
  );

  const previousIndex = usePrevious(movieIndex);

  useEffect(() => {
    if (
      movieIndex === -1 &&
      previousIndex !== -1 &&
      previousIndex !== undefined
    ) {
      history.push(`${window.Radarr.urlBase}/`);
    }
  }, [movieIndex, previousIndex, history]);

  if (movieIndex === -1) {
    return <NotFound message={translate('MovieCannotBeFound')} />;
  }

  return <MovieDetails movieId={allMovies[movieIndex].id} />;
}

export default MovieDetailsPage;
