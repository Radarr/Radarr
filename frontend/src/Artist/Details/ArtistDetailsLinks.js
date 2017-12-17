import PropTypes from 'prop-types';
import React from 'react';
import { kinds, sizes } from 'Helpers/Props';
import Label from 'Components/Label';
import Link from 'Components/Link/Link';
import styles from './ArtistDetailsLinks.css';

function ArtistDetailsLinks(props) {
  const {
    foreignArtistId,
    links
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

      {links.map((link, index) => {
        return (
          <span key={index}>
            <Link className={styles.link}
              to={link.url}
              key={index}
            >
              <Label
                className={styles.linkLabel}
                kind={kinds.INFO}
                size={sizes.LARGE}
              >
                {link.name}
              </Label>
            </Link>
            {(index > 0 && index % 5 === 0) &&
              <br />
            }

          </span>
        );
      })}

    </div>

  );
}

ArtistDetailsLinks.propTypes = {
  foreignArtistId: PropTypes.string.isRequired,
  links: PropTypes.arrayOf(PropTypes.object).isRequired
};

export default ArtistDetailsLinks;
