import React from 'react';
import AuthorHistoryContentConnector from 'Author/History/AuthorHistoryContentConnector';
import AuthorHistoryTableContent from 'Author/History/AuthorHistoryTableContent';

function AuthorHistoryTable(props) {
  const {
    ...otherProps
  } = props;

  return (
    <AuthorHistoryContentConnector
      component={AuthorHistoryTableContent}
      {...otherProps}
    />
  );
}

AuthorHistoryTable.propTypes = {
};

export default AuthorHistoryTable;
