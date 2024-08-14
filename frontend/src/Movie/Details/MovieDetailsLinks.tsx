import React from 'react';
import Label from 'Components/Label';
import Link from 'Components/Link/Link';
import { kinds, sizes } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './MovieDetailsLinks.css';

interface MovieDetailsLinksProps {
  tmdbId: number;
  imdbId?: string;
  youTubeTrailerId?: string;
}

function MovieDetailsLinks(props: MovieDetailsLinksProps) {
  const { tmdbId, imdbId, youTubeTrailerId } = props;

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
          {translate('TMDb')}
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
          {translate('Trakt')}
        </Label>
      </Link>

      <Link
        className={styles.link}
        to={`https://letterboxd.com/tmdb/${tmdbId}`}
      >
        <Label
          className={styles.linkLabel}
          kind={kinds.INFO}
          size={sizes.LARGE}
        >
          {translate('Letterboxd')}
        </Label>
      </Link>

      {imdbId ? (
        <Link className={styles.link} to={`https://imdb.com/title/${imdbId}/`}>
          <Label
            className={styles.linkLabel}
            kind={kinds.INFO}
            size={sizes.LARGE}
          >
            {translate('IMDb')}
          </Label>
        </Link>
      ) : null}

      {imdbId ? (
        <Link className={styles.link} to={`https://moviechat.org/${imdbId}/`}>
          <Label
            className={styles.linkLabel}
            kind={kinds.INFO}
            size={sizes.LARGE}
          >
            {translate('MovieChat')}
          </Label>
        </Link>
      ) : null}

      {youTubeTrailerId ? (
        <Link
          className={styles.link}
          to={`https://www.youtube.com/watch?v=${youTubeTrailerId}`}
        >
          <Label
            className={styles.linkLabel}
            kind={kinds.DANGER}
            size={sizes.LARGE}
          >
            {translate('Trailer')}
          </Label>
        </Link>
      ) : null}
    </div>
  );
}

export default MovieDetailsLinks;
