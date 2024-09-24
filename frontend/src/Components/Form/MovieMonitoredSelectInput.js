import PropTypes from 'prop-types';
import React from 'react';
import monitorOptions from 'Utilities/Movie/monitorOptions';
import translate from 'Utilities/String/translate';
import EnhancedSelectInput from './EnhancedSelectInput';

function MovieMonitoredSelectInput(props) {
  const {
    includeNoChange,
    includeMixed,
    ...otherProps
  } = props;

  const values = [...monitorOptions];

  if (includeNoChange) {
    values.unshift({
      key: 'noChange',
      get value() {
        return translate('NoChange');
      },
      isDisabled: true
    });
  }

  if (includeMixed) {
    values.unshift({
      key: 'mixed',
      get value() {
        return `(${translate('Mixed')})`;
      },
      isDisabled: true
    });
  }

  return (
    <EnhancedSelectInput
      {...otherProps}
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
