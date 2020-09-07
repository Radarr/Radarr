import PropTypes from 'prop-types';
import React from 'react';
import Label from 'Components/Label';
import Link from 'Components/Link/Link';
import { kinds, sizes } from 'Helpers/Props';
import styles from './BookDetailsLinks.css';

function BookDetailsLinks(props) {
  const {
    links
  } = props;

  return (
    <div className={styles.links}>

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

BookDetailsLinks.propTypes = {
  links: PropTypes.arrayOf(PropTypes.object).isRequired
};

export default BookDetailsLinks;
