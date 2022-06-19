import PropTypes from 'prop-types';
import React from 'react';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import { kinds } from 'Helpers/Props';

function OAuthInput(props) {
  const {
    className,
    label,
    authorizing,
    error,
    onPress
  } = props;

  return (
    <SpinnerErrorButton
      className={className}
      kind={kinds.PRIMARY}
      isSpinning={authorizing}
      error={error}
      onPress={onPress}
    >
      {label}
    </SpinnerErrorButton>
  );
}

OAuthInput.propTypes = {
  className: PropTypes.string,
  label: PropTypes.oneOfType([PropTypes.string, PropTypes.object]).isRequired,
  authorizing: PropTypes.bool.isRequired,
  error: PropTypes.object,
  onPress: PropTypes.func.isRequired
};

OAuthInput.defaultProps = {
  label: 'Start OAuth'
};

export default OAuthInput;
