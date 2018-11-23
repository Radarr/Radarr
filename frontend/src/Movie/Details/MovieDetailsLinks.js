import PropTypes from 'prop-types';
import React from 'react';
import { kinds, sizes } from 'Helpers/Props';
import Label from 'Components/Label';
import Link from 'Components/Link/Link';
import styles from './MovieDetailsLinks.css';

function MovieDetailsLinks(props) {
  const {
    tmdbId,
    imdbId
  } = props;

  return (
    <div className={styles.links}>
      <Link
        className={styles.link}
        to={`https://www.themoviedb.org/movie/${tmdbId}`}
      >
        <Label
          className={styles.linkLabel}
          kind={kinds.INFO}
          size={sizes.LARGE}
        >
          The Movie DB
        </Label>
      </Link>

      <Link
        className={styles.link}
        to={`http://trakt.tv/search/tvdb/${tmdbId}?id_type=show`}
      >
        <Label
          className={styles.linkLabel}
          kind={kinds.INFO}
          size={sizes.LARGE}
        >
          Trakt
        </Label>
      </Link>

      {
        !!imdbId &&
          <Link
            className={styles.link}
            to={`http://imdb.com/title/${imdbId}/`}
          >
            <Label
              className={styles.linkLabel}
              kind={kinds.INFO}
              size={sizes.LARGE}
            >
              IMDB
            </Label>
          </Link>
      }
    </div>
  );
}

MovieDetailsLinks.propTypes = {
  tmdbId: PropTypes.number.isRequired,
  imdbId: PropTypes.string
};

export default MovieDetailsLinks;
