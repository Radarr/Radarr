import PropTypes from 'prop-types';
import React from 'react';
import Label from 'Components/Label';
import Link from 'Components/Link/Link';
import { kinds, sizes } from 'Helpers/Props';
import styles from './MovieDetailsLinks.css';

function MovieDetailsLinks(props) {
  const {
    tmdbId,
    imdbId,
    youTubeTrailerId
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
          TMDb
        </Label>
      </Link>

      <Link
        className={styles.link}
        to={`https://trakt.tv/search/tmdb/${tmdbId}?id_type=movie`}
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
            to={`https://imdb.com/title/${imdbId}/`}
          >
            <Label
              className={styles.linkLabel}
              kind={kinds.INFO}
              size={sizes.LARGE}
            >
              IMDb
            </Label>
          </Link>
      }

      {
        !!imdbId &&
          <Link
            className={styles.link}
            to={` https://moviechat.org/${imdbId}/`}
          >
            <Label
              className={styles.linkLabel}
              kind={kinds.INFO}
              size={sizes.LARGE}
            >
              Movie Chat
            </Label>
          </Link>
      }

      {
        !!youTubeTrailerId &&
          <Link
            className={styles.link}
            to={` https://www.youtube.com/watch?v=${youTubeTrailerId}/`}
          >
            <Label
              className={styles.linkLabel}
              kind={kinds.DANGER}
              size={sizes.LARGE}
            >
              Trailer
            </Label>
          </Link>
      }
    </div>
  );
}

MovieDetailsLinks.propTypes = {
  tmdbId: PropTypes.number.isRequired,
  imdbId: PropTypes.string,
  youTubeTrailerId: PropTypes.string
};

export default MovieDetailsLinks;
