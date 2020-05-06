import PropTypes from 'prop-types';
import React from 'react';
import InteractiveSearchConnector from './InteractiveSearchConnector';

function InteractiveSearchTable(props) {
  const {
    type,
    ...otherProps
  } = props;

  return (
    <InteractiveSearchConnector
      searchPayload={otherProps}
      type={type}
    />
  );
}

InteractiveSearchTable.propTypes = {
  type: PropTypes.string.isRequired
};

export default InteractiveSearchTable;
