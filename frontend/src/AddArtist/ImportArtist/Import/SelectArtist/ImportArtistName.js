import PropTypes from 'prop-types';
import React from 'react';
import { kinds } from 'Helpers/Props';
import Label from 'Components/Label';
import styles from './ImportArtistName.css';

function ImportArtistName(props) {
  const {
    artistName,
    // year,
    isExistingArtist
  } = props;

  return (
    <div className={styles.artistNameContainer}>
      <div className={styles.artistName}>
        {artistName}
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
  // year: PropTypes.number.isRequired,
  isExistingArtist: PropTypes.bool.isRequired
};

export default ImportArtistName;
