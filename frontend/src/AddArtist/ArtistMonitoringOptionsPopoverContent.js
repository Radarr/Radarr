import React from 'react';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';

function ArtistMonitoringOptionsPopoverContent() {
  return (
    <DescriptionList>
      <DescriptionListItem
        title="All Albums"
        data="Monitor all albums except specials"
      />

      <DescriptionListItem
        title="Future Albums"
        data="Monitor albums that have not released yet"
      />

      <DescriptionListItem
        title="Missing Albums"
        data="Monitor albums that do not have files or have not released yet"
      />

      <DescriptionListItem
        title="Existing Albums"
        data="Monitor albums that have files or have not released yet"
      />

      <DescriptionListItem
        title="First Album"
        data="Monitor the first albums. All other albums will be ignored"
      />

      <DescriptionListItem
        title="Latest Album"
        data="Monitor the latest albums and future albums"
      />

      <DescriptionListItem
        title="None"
        data="No albums will be monitored"
      />
    </DescriptionList>
  );
}

export default ArtistMonitoringOptionsPopoverContent;
