import PropTypes from 'prop-types';
import React from 'react';
import Label from 'Components/Label';
import { kinds } from 'Helpers/Props';
import MoviePoster from 'Movie/MoviePoster';
import styles from './MovieSearchResult.css';

function MovieSearchResult(props) {
  const {
    match,
    title,
    year,
    images,
    alternateTitles,
    tmdbId,
    imdbId,
    tags
  } = props;

  let alternateTitle = null;
  let tag = null;

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
          {title} { year > 0 ? `(${year})` : ''}
        </div>

        {
          alternateTitle ?
            <div className={styles.alternateTitle}>
              {alternateTitle.title}
            </div> :
            null
        }

        {
          match.key === 'tmdbId' && tmdbId ?
            <div className={styles.alternateTitle}>
              TmdbId: {tmdbId}
            </div> :
            null
        }

        {
          match.key === 'imdbId' && imdbId ?
            <div className={styles.alternateTitle}>
              ImdbId: {imdbId}
            </div> :
            null
        }

        {
          tag ?
            <div className={styles.tagContainer}>
              <Label
                key={tag.id}
                kind={kinds.INFO}
              >
                {tag.label}
              </Label>
            </div> :
            null
        }
      </div>
    </div>
  );
}

MovieSearchResult.propTypes = {
  title: PropTypes.string.isRequired,
  year: PropTypes.number.isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  alternateTitles: PropTypes.arrayOf(PropTypes.object).isRequired,
  tmdbId: PropTypes.number,
  imdbId: PropTypes.string,
  tags: PropTypes.arrayOf(PropTypes.object).isRequired,
  match: PropTypes.object.isRequired
};

export default MovieSearchResult;
