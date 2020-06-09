import PropTypes from 'prop-types';
import React from 'react';
import { kinds } from 'Helpers/Props';
import Button from 'Components/Link/Button';
import styles from './NoDiscoverMovie.css';

function NoDiscoverMovie(props) {
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
        No list items or recommendations found, to get started you'll want to add a new movie, import some existing ones, or add a list.
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

      <div className={styles.buttonContainer}>
        <Button
          to="/settings/netimports"
          kind={kinds.PRIMARY}
        >
          Add List
        </Button>
      </div>
    </div>
  );
}

NoDiscoverMovie.propTypes = {
  totalItems: PropTypes.number.isRequired
};

export default NoDiscoverMovie;
