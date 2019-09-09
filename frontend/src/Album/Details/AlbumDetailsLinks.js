import PropTypes from 'prop-types';
import React from 'react';
import { kinds, sizes } from 'Helpers/Props';
import Label from 'Components/Label';
import Link from 'Components/Link/Link';
import styles from './AlbumDetailsLinks.css';

function AlbumDetailsLinks(props) {
  const {
    foreignAlbumId,
    links
  } = props;

  return (
    <div className={styles.links}>

      <Link
        className={styles.link}
        to={`https://musicbrainz.org/release-group/${foreignAlbumId}`}
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

AlbumDetailsLinks.propTypes = {
  foreignAlbumId: PropTypes.string.isRequired,
  links: PropTypes.arrayOf(PropTypes.object).isRequired
};

export default AlbumDetailsLinks;
