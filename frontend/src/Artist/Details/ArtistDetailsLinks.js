import PropTypes from 'prop-types';
import React from 'react';
import { kinds, sizes } from 'Helpers/Props';
import Label from 'Components/Label';
import Link from 'Components/Link/Link';
import styles from './ArtistDetailsLinks.css';

function ArtistDetailsLinks(props) {
  const {
    foreignArtistId
  } = props;

  return (
    <div className={styles.links}>
      <Link
        className={styles.link}
        to={`https://musicbrainz.org/artist/${foreignArtistId}`}
      >
        <Label
          className={styles.linkLabel}
          kind={kinds.INFO}
          size={sizes.LARGE}
        >
          Musicbrainz
        </Label>
      </Link>

    </div>
  );
}

ArtistDetailsLinks.propTypes = {
  foreignArtistId: PropTypes.string.isRequired
};

export default ArtistDetailsLinks;
