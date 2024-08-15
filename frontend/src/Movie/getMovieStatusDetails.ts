import { icons } from 'Helpers/Props';
import { MovieStatus } from 'Movie/Movie';
import translate from 'Utilities/String/translate';

export default function getMovieStatusDetails(status: MovieStatus) {
  let statusDetails = {
    icon: icons.ANNOUNCED,
    title: translate('Announced'),
    message: translate('AnnouncedMovieDescription'),
  };

  if (status === 'deleted') {
    statusDetails = {
      icon: icons.MOVIE_DELETED,
      title: translate('Deleted'),
      message: translate('DeletedMovieDescription'),
    };
  } else if (status === 'inCinemas') {
    statusDetails = {
      icon: icons.IN_CINEMAS,
      title: translate('InCinemas'),
      message: translate('InCinemasMovieDescription'),
    };
  } else if (status === 'released') {
    statusDetails = {
      icon: icons.MOVIE_FILE,
      title: translate('Released'),
      message: translate('ReleasedMovieDescription'),
    };
  }

  return statusDetails;
}
