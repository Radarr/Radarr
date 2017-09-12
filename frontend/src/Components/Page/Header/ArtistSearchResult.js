import _ from 'lodash';
import PropTypes from 'prop-types';
import React from 'react';
import ArtistPoster from 'Artist/ArtistPoster';
import styles from './ArtistSearchResult.css';

function getMatchingAlternateTile(alternateTitles, query) {
  return _.first(alternateTitles, (alternateTitle) => {
    return alternateTitle.title.toLowerCase().contains(query.toLowerCase());
  });
}

function ArtistSearchResult(props) {
  const {
    query,
    artistName,
    // alternateTitles,
    images
  } = props;

  const index = artistName.toLowerCase().indexOf(query.toLowerCase());
  // const alternateTitle = index === -1 ?
  //   getMatchingAlternateTile(alternateTitles, query) :
  //   null;

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
          // !!alternateTitle &&
          // <div className={styles.alternateTitle}>
          //   {alternateTitle.title}
          // </div>
        }
      </div>
    </div>
  );
}

ArtistSearchResult.propTypes = {
  query: PropTypes.string.isRequired,
  artistName: PropTypes.string.isRequired,
  // alternateTitles: PropTypes.arrayOf(PropTypes.object).isRequired,
  images: PropTypes.arrayOf(PropTypes.object).isRequired
};

export default ArtistSearchResult;
