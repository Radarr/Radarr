import PropTypes from 'prop-types';
import React from 'react';
import monitorOptions from 'Utilities/Author/monitorOptions';
import SelectInput from './SelectInput';

function MonitorBooksSelectInput(props) {
  const {
    includeNoChange,
    includeMixed,
    ...otherProps
  } = props;

  const values = [...monitorOptions];

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
      values={values}
      {...otherProps}
    />
  );
}

MonitorBooksSelectInput.propTypes = {
  includeNoChange: PropTypes.bool.isRequired,
  includeMixed: PropTypes.bool.isRequired,
  onChange: PropTypes.func.isRequired
};

MonitorBooksSelectInput.defaultProps = {
  includeNoChange: false,
  includeMixed: false
};

export default MonitorBooksSelectInput;
