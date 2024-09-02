import React from 'react';
import Button from 'Components/Link/Button';
import { kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './NoMovie.css';

interface NoMovieProps {
  totalItems: number;
}

function NoMovie(props: NoMovieProps) {
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
      <div className={styles.message}>{translate('NoMoviesExist')}</div>

      <div className={styles.buttonContainer}>
        <Button to="/add/import" kind={kinds.PRIMARY}>
          {translate('ImportExistingMovies')}
        </Button>
      </div>

      <div className={styles.buttonContainer}>
        <Button to="/add/new" kind={kinds.PRIMARY}>
          {translate('AddNewMovie')}
        </Button>
      </div>
    </div>
  );
}

export default NoMovie;
