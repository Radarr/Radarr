import React from 'react';
import ArtistHistoryContentConnector from 'Artist/History/ArtistHistoryContentConnector';
import ArtistHistoryTableContent from 'Artist/History/ArtistHistoryTableContent';

function ArtistHistoryTable(props) {
  const {
    ...otherProps
  } = props;

  return (
    <ArtistHistoryContentConnector
      component={ArtistHistoryTableContent}
      {...otherProps}
    />
  );
}

ArtistHistoryTable.propTypes = {
};

export default ArtistHistoryTable;
