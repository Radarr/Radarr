import PropTypes from 'prop-types';
import React from 'react';
import { kinds } from 'Helpers/Props';
import Button from 'Components/Link/Button';
import styles from './NoArtist.css';

function NoArtist(props) {
  const { totalItems } = props;

  if (totalItems > 0) {
    return (
      <div>
        <div className={styles.message}>
        All artists are hidden due to the applied filter.
        </div>
      </div>
    );
  }

  return (
    <div>
      <div className={styles.message}>
        No artist found, to get started you'll want to add a new artist or import some existing ones.
      </div>

      <div className={styles.buttonContainer}>
        <Button
          to="/add/import"
          kind={kinds.PRIMARY}
        >
          Import Existing Artist(s)
        </Button>
      </div>

      <div className={styles.buttonContainer}>
        <Button
          to="/add/new"
          kind={kinds.PRIMARY}
        >
          Add New Artist
        </Button>
      </div>
    </div>
  );
}

NoArtist.propTypes = {
  totalItems: PropTypes.number.isRequired
};

export default NoArtist;
