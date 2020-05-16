import PropTypes from 'prop-types';
import React from 'react';
import { kinds } from 'Helpers/Props';
import Button from 'Components/Link/Button';
import styles from './NoAuthor.css';

function NoAuthor(props) {
  const { totalItems } = props;

  if (totalItems > 0) {
    return (
      <div>
        <div className={styles.message}>
        All authors are hidden due to the applied filter.
        </div>
      </div>
    );
  }

  return (
    <div>
      <div className={styles.message}>
        No authors found, to get started you'll want to add a new author or book or add an existing library location (Root Folder) and update.
      </div>

      <div className={styles.buttonContainer}>
        <Button
          to="/settings/mediamanagement"
          kind={kinds.PRIMARY}
        >
          Add Root Folder
        </Button>
      </div>

      <div className={styles.buttonContainer}>
        <Button
          to="/add/search"
          kind={kinds.PRIMARY}
        >
          Add New Author
        </Button>
      </div>
    </div>
  );
}

NoAuthor.propTypes = {
  totalItems: PropTypes.number.isRequired
};

export default NoAuthor;
