import React from 'react';
import getProgressBarKind from 'Utilities/Artist/getProgressBarKind';
import ProgressBar from 'Components/ProgressBar';
import VirtualTableRowCell from 'Components/Table/Cells/VirtualTableRowCell';
import RelativeDateCellConnector from 'Components/Table/Cells/RelativeDateCellConnector';
import QualityProfileNameConnector from 'Settings/Profiles/Quality/QualityProfileNameConnector';
import ArtistNameLink from 'Artist/ArtistNameLink';
import ArtistIndexItemConnector from 'Artist/Index/ArtistIndexItemConnector';
import ArtistIndexActionsCell from './ArtistIndexActionsCell';
import ArtistStatusCell from './ArtistStatusCell';

export default function artistIndexCellRenderers(cellProps) {
  const {
    cellKey,
    dataKey,
    rowData,
    ...otherProps
  } = cellProps;

  const {
    id,
    monitored,
    status,
    name,
    nameSlug,
    foreignArtistId,
    qualityProfileId,
    nextAiring,
    previousAiring,
    albumCount,
    trackCount,
    trackFileCount
  } = rowData;

  const progress = trackCount ? trackFileCount / trackCount * 100 : 100;

  if (dataKey === 'status') {
    return (
      <ArtistStatusCell
        key={cellKey}
        monitored={monitored}
        status={status}
        component={VirtualTableRowCell}
        {...otherProps}
      />
    );
  }

  if (dataKey === 'sortName') {
    return (
      <VirtualTableRowCell
        key={cellKey}
        {...otherProps}
      >
        <ArtistNameLink
          foreignArtistId={foreignArtistId}
          name={name}
        />
      </VirtualTableRowCell>

    );
  }

  if (dataKey === 'qualityProfileId') {
    return (
      <VirtualTableRowCell
        key={cellKey}
        {...otherProps}
      >
        <QualityProfileNameConnector
          qualityProfileId={qualityProfileId}
        />
      </VirtualTableRowCell>
    );
  }

  if (dataKey === 'nextAiring') {
    return (
      <RelativeDateCellConnector
        key={cellKey}
        date={nextAiring}
        component={VirtualTableRowCell}
        {...otherProps}
      />
    );
  }

  if (dataKey === 'albumCount') {
    return (
      <VirtualTableRowCell
        key={cellKey}
        {...otherProps}
      >
        {albumCount}
      </VirtualTableRowCell>
    );
  }

  if (dataKey === 'trackProgress') {
    return (
      <VirtualTableRowCell
        key={cellKey}
        {...otherProps}
      >
        <ProgressBar
          progress={progress}
          kind={getProgressBarKind(status, monitored, progress)}
          showText={true}
          text={`${trackFileCount} / ${trackCount}`}
          width={125}
        />
      </VirtualTableRowCell>
    );
  }

  if (dataKey === 'actions') {
    return (
      <ArtistIndexItemConnector
        key={cellKey}
        component={ArtistIndexActionsCell}
        id={id}
        {...otherProps}
      />
    );
  }
}
