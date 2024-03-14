import PropTypes from 'prop-types';
import React from 'react';
import translate from 'Utilities/String/translate';
import EnhancedSelectInput from './EnhancedSelectInput';

const availabilityOptions = [
  {
    key: 'announced',
    get value() {
      return translate('Announced');
    }
  },
  {
    key: 'inCinemas',
    get value() {
      return translate('InCinemas');
    }
  },
  {
    key: 'released',
    get value() {
      return translate('Released');
    }
  }
];

function AvailabilitySelectInput(props) {
  const values = [...availabilityOptions];

  const {
    includeNoChange,
    includeMixed
  } = props;

  if (includeNoChange) {
    values.unshift({
      key: 'noChange',
      value: translate('NoChange'),
      isDisabled: true
    });
  }

  if (includeMixed) {
    values.unshift({
      key: 'mixed',
      value: '(Mixed)',
      isDisabled: true
    });
  }

  return (
    <EnhancedSelectInput
      {...props}
      values={values}
    />
  );
}

AvailabilitySelectInput.propTypes = {
  includeNoChange: PropTypes.bool.isRequired,
  includeMixed: PropTypes.bool.isRequired
};

AvailabilitySelectInput.defaultProps = {
  includeNoChange: false,
  includeMixed: false
};

export default AvailabilitySelectInput;
