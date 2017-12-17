import moment from 'moment';
import PropTypes from 'prop-types';
import React from 'react';
import isInNextWeek from 'Utilities/Date/isInNextWeek';
import isToday from 'Utilities/Date/isToday';
import isTomorrow from 'Utilities/Date/isTomorrow';
import { kinds, sizes } from 'Helpers/Props';
import Label from 'Components/Label';

function EpisodeAiring(props) {
  const {
    releaseDate,
    albumLabel,
    shortDateFormat,
    showRelativeDates
  } = props;

  const networkLabel = (
    <Label
      kind={kinds.INFO}
      size={sizes.MEDIUM}
    >
      {albumLabel}
    </Label>
  );

  if (!releaseDate) {
    return (
      <span>
        TBA on {networkLabel}
      </span>
    );
  }

  if (!showRelativeDates) {
    return (
      <span>
        {moment(releaseDate).format(shortDateFormat)} on {networkLabel}
      </span>
    );
  }

  if (isToday(releaseDate)) {
    return (
      <span>
        Today on {networkLabel}
      </span>
    );
  }

  if (isTomorrow(releaseDate)) {
    return (
      <span>
        Tomorrow on {networkLabel}
      </span>
    );
  }

  if (isInNextWeek(releaseDate)) {
    return (
      <span>
        {moment(releaseDate).format('dddd')} on {networkLabel}
      </span>
    );
  }

  return (
    <span>
      {moment(releaseDate).format(shortDateFormat)} on {networkLabel}
    </span>
  );
}

EpisodeAiring.propTypes = {
  releaseDate: PropTypes.string.isRequired,
  albumLabel: PropTypes.arrayOf(PropTypes.string).isRequired,
  shortDateFormat: PropTypes.string.isRequired,
  showRelativeDates: PropTypes.bool.isRequired
};

export default EpisodeAiring;
