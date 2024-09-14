import React from 'react';
import { useSelector } from 'react-redux';
import createMovieCreditsSelector from 'Store/Selectors/createMovieCreditsSelector';
import MovieCreditPosters from '../MovieCreditPosters';
import MovieCrewPoster from './MovieCrewPoster';

interface MovieCrewPostersProps {
  isSmallScreen: boolean;
}

function MovieCrewPosters({ isSmallScreen }: MovieCrewPostersProps) {
  const { items: crewCredits } = useSelector(
    createMovieCreditsSelector('crew')
  );

  return (
    <MovieCreditPosters
      items={crewCredits}
      itemComponent={MovieCrewPoster}
      isSmallScreen={isSmallScreen}
    />
  );
}

export default MovieCrewPosters;
