import PropTypes from 'prop-types';
import React from 'react';
import monitorOptions from 'Utilities/Movie/monitorOptions';
import translate from 'Utilities/String/translate';
import EnhancedSelectInput from './EnhancedSelectInput';

function MovieMonitoredSelectInput(props) {
  const values = [...monitorOptions];

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

MovieMonitoredSelectInput.propTypes = {
  includeNoChange: PropTypes.bool.isRequired,
  includeMixed: PropTypes.bool.isRequired
};

MovieMonitoredSelectInput.defaultProps = {
  includeNoChange: false,
  includeMixed: false
};

export default MovieMonitoredSelectInput;
