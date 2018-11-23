import PropTypes from 'prop-types';
import React from 'react';
import { kinds } from 'Helpers/Props';
import Button from 'Components/Link/Button';
import styles from './NoMovie.css';

function NoMovie(props) {
  const { totalItems } = props;

  if (totalItems > 0) {
    return (
      <div>
        <div className={styles.message}>
        All movies are hidden due to the applied filter.
        </div>
      </div>
    );
  }

  return (
    <div>
      <div className={styles.message}>
        No movies found, to get started you'll want to add a new movie or import some existing ones.
      </div>

      <div className={styles.buttonContainer}>
        <Button
          to="/add/import"
          kind={kinds.PRIMARY}
        >
          Import Existing Movies
        </Button>
      </div>

      <div className={styles.buttonContainer}>
        <Button
          to="/add/new"
          kind={kinds.PRIMARY}
        >
          Add New Movie
        </Button>
      </div>
    </div>
  );
}

NoMovie.propTypes = {
  totalItems: PropTypes.number.isRequired
};

export default NoMovie;
