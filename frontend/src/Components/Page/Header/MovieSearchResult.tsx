import React from 'react';
import { Tag } from 'App/State/TagsAppState';
import Label from 'Components/Label';
import { kinds } from 'Helpers/Props';
import MoviePoster from 'Movie/MoviePoster';
import { SuggestedMovie } from './MovieSearchInput';
import styles from './MovieSearchResult.css';

interface Match {
  key: string;
  refIndex: number;
}

interface MovieSearchResultProps extends SuggestedMovie {
  match: Match;
}

function MovieSearchResult(props: MovieSearchResultProps) {
  const { match, title, year, images, alternateTitles, tmdbId, imdbId, tags } =
    props;

  let alternateTitle = null;
  let tag: Tag | null = null;

  if (match.key === 'alternateTitles.title') {
    alternateTitle = alternateTitles[match.refIndex];
  } else if (match.key === 'tags.label') {
    tag = tags[match.refIndex];
  }

  return (
    <div className={styles.result}>
      <MoviePoster
        className={styles.poster}
        images={images}
        size={250}
        lazy={false}
        overflow={true}
      />

      <div className={styles.titles}>
        <div className={styles.title}>
          {title} {year > 0 ? `(${year})` : ''}
        </div>

        {alternateTitle ? (
          <div className={styles.alternateTitle}>{alternateTitle.title}</div>
        ) : null}

        {match.key === 'tmdbId' && tmdbId ? (
          <div className={styles.alternateTitle}>TmdbId: {tmdbId}</div>
        ) : null}

        {match.key === 'imdbId' && imdbId ? (
          <div className={styles.alternateTitle}>ImdbId: {imdbId}</div>
        ) : null}

        {tag ? (
          <div className={styles.tagContainer}>
            <Label key={tag.id} kind={kinds.INFO}>
              {tag.label}
            </Label>
          </div>
        ) : null}
      </div>
    </div>
  );
}

export default MovieSearchResult;
