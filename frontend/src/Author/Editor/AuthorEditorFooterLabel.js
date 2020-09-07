import PropTypes from 'prop-types';
import React from 'react';
import SpinnerIcon from 'Components/SpinnerIcon';
import { icons } from 'Helpers/Props';
import styles from './AuthorEditorFooterLabel.css';

function AuthorEditorFooterLabel(props) {
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

AuthorEditorFooterLabel.propTypes = {
  className: PropTypes.string.isRequired,
  label: PropTypes.string.isRequired,
  isSaving: PropTypes.bool.isRequired
};

AuthorEditorFooterLabel.defaultProps = {
  className: styles.label
};

export default AuthorEditorFooterLabel;
