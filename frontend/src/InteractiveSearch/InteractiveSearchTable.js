import React from 'react';
import InteractiveSearchContentConnector from './InteractiveSearchContentConnector';

function InteractiveSearchTable(props) {

  return (
    <InteractiveSearchContentConnector
      searchPayload={props}
    />
  );
}

InteractiveSearchTable.propTypes = {
};

export default InteractiveSearchTable;
