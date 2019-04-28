import PropTypes from 'prop-types';
import React from 'react';
import { kinds } from 'Helpers/Props';
import Label from 'Components/Label';
import ArtistPoster from 'Artist/ArtistPoster';
import styles from './ArtistSearchResult.css';

function ArtistSearchResult(props) {
  const {
    match,
    artistName,
    images,
    tags
  } = props;

  let tag = null;

  if (match.key === 'tags.label') {
    tag = tags[match.arrayIndex];
  }

  return (
    <div className={styles.result}>
      <ArtistPoster
        className={styles.poster}
        images={images}
        size={250}
        lazy={false}
        overflow={true}
      />

      <div className={styles.titles}>
        <div className={styles.title}>
          {artistName}
        </div>

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

ArtistSearchResult.propTypes = {
  artistName: PropTypes.string.isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  tags: PropTypes.arrayOf(PropTypes.object).isRequired,
  match: PropTypes.object.isRequired
};

export default ArtistSearchResult;
