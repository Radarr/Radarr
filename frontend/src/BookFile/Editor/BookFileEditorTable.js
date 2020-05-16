import React from 'react';
import BookFileEditorTableContentConnector from './BookFileEditorTableContentConnector';

function BookFileEditorTable(props) {
  const {
    ...otherProps
  } = props;

  return (
    <BookFileEditorTableContentConnector
      {...otherProps}
    />
  );
}

export default BookFileEditorTable;
