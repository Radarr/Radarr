import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';

export function getMovieStatusDetails(status) {

  let statusDetails = {
    icon: icons.ANNOUNCED,
    title: translate('Announced'),
    message: translate('AnnouncedMsg')
  };

  if (status === 'deleted') {
    statusDetails = {
      icon: icons.MOVIE_DELETED,
      title: translate('Deleted'),
      message: translate('DeletedMsg')
    };
  } else if (status === 'inCinemas') {
    statusDetails = {
      icon: icons.IN_CINEMAS,
      title: translate('InCinemas'),
      message: translate('InCinemasMsg')
    };
  } else if (status === 'released') {
    statusDetails = {
      icon: icons.MOVIE_FILE,
      title: translate('Released'),
      message: translate('ReleasedMsg')
    };
  }

  return statusDetails;
}
