import PropTypes from 'prop-types';
import React from 'react';
import translate from 'Utilities/String/translate';
import SelectInput from './SelectInput';

const availabilityOptions = [
  { key: 'announced', value: translate('Announced') },
  { key: 'inCinemas', value: translate('InCinemas') },
  { key: 'released', value: translate('Released') }
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
      value: 'No Change',
      disabled: true
    });
  }

  if (includeMixed) {
    values.unshift({
      key: 'mixed',
      value: '(Mixed)',
      disabled: true
    });
  }

  return (
    <SelectInput
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
