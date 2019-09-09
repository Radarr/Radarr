import PropTypes from 'prop-types';
import React from 'react';
import { kinds } from 'Helpers/Props';
import Label from 'Components/Label';
import styles from './ImportArtistName.css';

function ImportArtistName(props) {
  const {
    artistName,
    disambiguation,
    isExistingArtist
  } = props;

  return (
    <div className={styles.artistNameContainer}>
      <div className={styles.artistName}>
        {artistName}
      </div>
      <div className={styles.disambiguation}>
        {disambiguation}
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
  disambiguation: PropTypes.string,
  isExistingArtist: PropTypes.bool.isRequired
};

export default ImportArtistName;
