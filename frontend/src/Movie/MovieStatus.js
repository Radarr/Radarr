import { icons } from 'Helpers/Props';

export function getMovieStatusDetails(status) {

  let statusDetails = {
    icon: icons.ANNOUNCED,
    title: 'Announced',
    message: 'Movie is announced'
  };

  if (status === 'deleted') {
    statusDetails = {
      icon: icons.MOVIE_DELETED,
      title: 'Deleted',
      message: 'Movie was deleted from TMDb'
    };
  } else if (status === 'inCinemas') {
    statusDetails = {
      icon: icons.IN_CINEMAS,
      title: 'In Cinemas',
      message: 'Movie is in Cinemas'
    };
  } else if (status === 'released') {
    statusDetails = {
      icon: icons.MOVIE_FILE,
      title: 'Released',
      message: 'Movie is released'
    };
  }

  return statusDetails;
}
