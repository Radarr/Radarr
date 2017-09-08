import PropTypes from 'prop-types';
import React from 'react';
import { kinds } from 'Helpers/Props';
import Label from 'Components/Label';
import styles from './ImportArtistName.css';

function ImportArtistName(props) {
  const {
    artistName,
    overview,
    // year,
    // network,
    isExistingArtist
  } = props;

  return (
    <div className={styles.artistNameContainer}>
      <div className={styles.artistName}>
        {artistName}
      </div>
      <div className={styles.overview}>
        {overview}
      </div>

      {
        isExistingArtist &&
          <Label
            kind={kinds.WARNING}
          >
            Existing
          </Label>
      }
    </div>
  );
}

ImportArtistName.propTypes = {
  artistName: PropTypes.string.isRequired,
  overview: PropTypes.string.isRequired,
  // year: PropTypes.number.isRequired,
  // network: PropTypes.string,
  isExistingArtist: PropTypes.bool.isRequired
};

export default ImportArtistName;
