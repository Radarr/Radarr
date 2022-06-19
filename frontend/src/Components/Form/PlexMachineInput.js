import PropTypes from 'prop-types';
import React from 'react';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import SelectInput from './SelectInput';

function PlexMachineInput(props) {
  const {
    isFetching,
    isDisabled,
    value,
    values,
    onChange,
    ...otherProps
  } = props;

  const helpText = 'Authenticate with plex.tv to show servers to use for authentication';

  return (
    <>
      {
        isFetching ?
          <LoadingIndicator /> :
          <SelectInput
            value={value}
            values={values}
            isDisabled={isDisabled}
            onChange={onChange}
            helpText={helpText}
            {...otherProps}
          />
      }
    </>
  );
}

PlexMachineInput.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  isDisabled: PropTypes.bool.isRequired,
  value: PropTypes.string,
  values: PropTypes.arrayOf(PropTypes.object).isRequired,
  onChange: PropTypes.func.isRequired
};

export default PlexMachineInput;
