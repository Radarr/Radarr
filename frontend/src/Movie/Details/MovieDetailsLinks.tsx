import React, { useMemo } from 'react';
import Label, { LabelProps } from 'Components/Label';
import ClipboardButton from 'Components/Link/ClipboardButton';
import Link from 'Components/Link/Link';
import { kinds, sizes } from 'Helpers/Props';
import Movie from 'Movie/Movie';
import translate from 'Utilities/String/translate';
import styles from './MovieDetailsLinks.css';

type MovieDetailsLinksProps = Pick<
  Movie,
  'tmdbId' | 'imdbId' | 'youTubeTrailerId'
>;

interface MovieDetailsLink {
  externalId?: string | number;
  name: string;
  url: string;
  kind?: LabelProps['kind'];
}

function MovieDetailsLinks(props: MovieDetailsLinksProps) {
  const { tmdbId, imdbId, youTubeTrailerId } = props;

  const links = useMemo(() => {
    const validLinks: MovieDetailsLink[] = [];

    if (tmdbId) {
      validLinks.push(
        {
          externalId: tmdbId,
          name: 'TMDb',
          url: `https://www.themoviedb.org/movie/${tmdbId}`,
        },
        {
          name: 'Letterboxd',
          url: `https://letterboxd.com/tmdb/${tmdbId}`,
        }
      );
    }

    if (imdbId) {
      validLinks.push(
        {
          externalId: imdbId,
          name: 'IMDb',
          url: `https://imdb.com/title/${imdbId}/`,
        },
        {
          name: 'Trakt',
          url: `https://trakt.tv/movies/${imdbId}`,
        },
        {
          name: 'Movie Chat',
          url: `https://moviechat.org/${imdbId}/`,
        },
        {
          name: 'MDBList',
          url: `https://mdblist.com/movie/${imdbId}`,
        },
        {
          name: 'Blu-ray',
          url: `https://www.blu-ray.com/search/?quicksearch=1&quicksearch_keyword=${imdbId}&section=theatrical`,
        }
      );
    }

    if (youTubeTrailerId) {
      validLinks.push({
        name: translate('Trailer'),
        url: `https://www.youtube.com/watch?v=${youTubeTrailerId}`,
        kind: kinds.DANGER,
      });
    }

    return validLinks.sort(
      (a, b) => Number(!a.externalId) - Number(!b.externalId)
    );
  }, [tmdbId, imdbId, youTubeTrailerId]);

  return (
    <div className={styles.links}>
      {links.map((link) => (
        <div key={link.name} className={styles.linkBlock}>
          <Link className={styles.link} to={link.url}>
            <Label
              className={styles.linkLabel}
              kind={link.kind ?? kinds.INFO}
              size={sizes.LARGE}
            >
              {link.name}
            </Label>
          </Link>

          {link.externalId ? (
            <ClipboardButton
              value={`${link.externalId}`}
              title={translate('CopyToClipboard')}
              kind={kinds.DEFAULT}
              size={sizes.SMALL}
              label={link.externalId}
            />
          ) : null}
        </div>
      ))}
    </div>
  );
}

export default MovieDetailsLinks;
