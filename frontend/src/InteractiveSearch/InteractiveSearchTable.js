import React from 'react';
import InteractiveSearchContentConnector from './InteractiveSearchContentConnector';

function InteractiveSearchTable(props) {
  // const {
  //   ...otherProps
  // } = props;

  return (
  // <InteractiveSearchTableContent
  // {...otherProps}
  // />
    <InteractiveSearchContentConnector
      searchPayload={props}
    />
  );
}

InteractiveSearchTable.propTypes = {
};

export default InteractiveSearchTable;
