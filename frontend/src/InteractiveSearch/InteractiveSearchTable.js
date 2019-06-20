import PropTypes from 'prop-types';
import React from 'react';
import InteractiveSearchTableContent from './InteractiveSearchTableContent';

function InteractiveSearchTable(props) {
  const {
    ...otherProps
  } = props;

  return (
    <InteractiveSearchTableContent
    {...otherProps}
    />
  );
}

InteractiveSearchTable.propTypes = {
};

export default InteractiveSearchTable;
