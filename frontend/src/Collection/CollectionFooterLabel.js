import PropTypes from 'prop-types';
import React from 'react';
import SpinnerIcon from 'Components/SpinnerIcon';
import { icons } from 'Helpers/Props';
import styles from './CollectionFooterLabel.css';

function CollectionFooterLabel(props) {
  const {
    className,
    label,
    isSaving
  } = props;

  return (
    <div className={className}>
      {label}

      {
        isSaving &&
          <SpinnerIcon
            className={styles.savingIcon}
            name={icons.SPINNER}
            isSpinning={true}
          />
      }
    </div>
  );
}

CollectionFooterLabel.propTypes = {
  className: PropTypes.string.isRequired,
  label: PropTypes.string.isRequired,
  isSaving: PropTypes.bool.isRequired
};

CollectionFooterLabel.defaultProps = {
  className: styles.label
};

export default CollectionFooterLabel;
