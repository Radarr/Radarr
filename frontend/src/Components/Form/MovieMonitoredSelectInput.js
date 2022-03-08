import PropTypes from 'prop-types';
import React from 'react';
import monitorOptions from 'Utilities/Movie/monitorOptions';
import SelectInput from './SelectInput';

function MovieMonitoredSelectInput(props) {
  const values = [...monitorOptions];

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
