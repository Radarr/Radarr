import PropTypes from 'prop-types';
import React from 'react';
import SelectInput from './SelectInput';

const monitorTypesOptions = [
  { key: 'true', value: 'True' },
  { key: 'false', value: 'False' }
];

function MovieMonitoredSelectInput(props) {
  const values = [...monitorTypesOptions];

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

MovieMonitoredSelectInput.propTypes = {
  includeNoChange: PropTypes.bool.isRequired,
  includeMixed: PropTypes.bool.isRequired
};

MovieMonitoredSelectInput.defaultProps = {
  includeNoChange: false,
  includeMixed: false
};

export default MovieMonitoredSelectInput;
