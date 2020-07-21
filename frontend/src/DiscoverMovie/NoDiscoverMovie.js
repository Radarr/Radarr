import PropTypes from 'prop-types';
import React from 'react';
import Button from 'Components/Link/Button';
import { kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './NoDiscoverMovie.css';

function NoDiscoverMovie(props) {
  const { totalItems } = props;

  if (totalItems > 0) {
    return (
      <div>
        <div className={styles.message}>
          {translate('AllMoviesHiddenDueToFilter')}
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
          {translate('ImportExistingMovies')}
        </Button>
      </div>

      <div className={styles.buttonContainer}>
        <Button
          to="/add/new"
          kind={kinds.PRIMARY}
        >
          {translate('AddNewMovie')}
        </Button>
      </div>

      <div className={styles.buttonContainer}>
        <Button
          to="/settings/netimports"
          kind={kinds.PRIMARY}
        >
          {translate('AddList')}
        </Button>
      </div>
    </div>
  );
}

NoDiscoverMovie.propTypes = {
  totalItems: PropTypes.number.isRequired
};

export default NoDiscoverMovie;
