import PropTypes from 'prop-types';
import React from 'react';
import { icons } from 'Helpers/Props';
import SpinnerIcon from 'Components/SpinnerIcon';
import styles from './ArtistEditorFooterLabel.css';

function ArtistEditorFooterLabel(props) {
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

ArtistEditorFooterLabel.propTypes = {
  className: PropTypes.string.isRequired,
  label: PropTypes.string.isRequired,
  isSaving: PropTypes.bool.isRequired
};

ArtistEditorFooterLabel.defaultProps = {
  className: styles.label
};

export default ArtistEditorFooterLabel;
