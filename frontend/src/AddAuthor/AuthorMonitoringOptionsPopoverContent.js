import React from 'react';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';

function AuthorMonitoringOptionsPopoverContent() {
  return (
    <DescriptionList>
      <DescriptionListItem
        title="All Books"
        data="Monitor all books"
      />

      <DescriptionListItem
        title="Future Books"
        data="Monitor books that have not released yet"
      />

      <DescriptionListItem
        title="Missing Books"
        data="Monitor books that do not have files or have not released yet"
      />

      <DescriptionListItem
        title="Existing Books"
        data="Monitor books that have files or have not released yet"
      />

      <DescriptionListItem
        title="First Book"
        data="Monitor the first book. All other books will be ignored"
      />

      <DescriptionListItem
        title="Latest Book"
        data="Monitor the latest book and future books"
      />

      <DescriptionListItem
        title="None"
        data="No books will be monitored"
      />
    </DescriptionList>
  );
}

export default AuthorMonitoringOptionsPopoverContent;
