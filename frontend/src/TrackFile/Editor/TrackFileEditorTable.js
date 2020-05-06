import React from 'react';
import TrackFileEditorTableContentConnector from './TrackFileEditorTableContentConnector';

function TrackFileEditorTable(props) {
  const {
    ...otherProps
  } = props;

  return (
    <TrackFileEditorTableContentConnector
      {...otherProps}
    />
  );
}

export default TrackFileEditorTable;
