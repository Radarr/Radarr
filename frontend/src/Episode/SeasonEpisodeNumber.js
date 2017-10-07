import PropTypes from 'prop-types';
import React from 'react';
import padNumber from 'Utilities/Number/padNumber';
import styles from './SeasonEpisodeNumber.css';

function SeasonEpisodeNumber(props) {
  const {
    seasonNumber,
    episodeNumber,
    absoluteEpisodeNumber,
    airDate,
    artistType
  } = props;

  if (artistType === 'daily' && airDate) {
    return (
      <span>{airDate}</span>
    );
  }

  if (artistType === 'anime') {
    return (
      <span>
        {seasonNumber}x{padNumber(episodeNumber, 2)}

        {
          absoluteEpisodeNumber &&
            <span className={styles.absoluteEpisodeNumber}>
              ({absoluteEpisodeNumber})
            </span>
        }
      </span>
    );
  }

  return (
    <span>
      {seasonNumber}x{padNumber(episodeNumber, 2)}
    </span>
  );
}

SeasonEpisodeNumber.propTypes = {
  seasonNumber: PropTypes.number.isRequired,
  episodeNumber: PropTypes.number.isRequired,
  absoluteEpisodeNumber: PropTypes.number,
  airDate: PropTypes.string,
  artistType: PropTypes.string
};

export default SeasonEpisodeNumber;
