import React from 'react';
import Audiobook from 'Audiobook/Audiobook';
import Icon from 'Components/Icon';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRow from 'Components/Table/TableRow';
import { icons } from 'Helpers/Props';
import formatDate from 'Utilities/Date/formatDate';

function formatDuration(minutes: number | undefined): string {
  if (!minutes) {
    return '-';
  }

  const hours = Math.floor(minutes / 60);
  const mins = minutes % 60;

  if (hours > 0) {
    return `${hours}h ${mins}m`;
  }

  return `${mins}m`;
}

function AudiobookIndexRow(props: Audiobook) {
  const { title, narrator, durationMinutes, releaseDate, monitored } = props;

  return (
    <TableRow>
      <TableRowCell>{title}</TableRowCell>
      <TableRowCell>{narrator || '-'}</TableRowCell>
      <TableRowCell>{formatDuration(durationMinutes)}</TableRowCell>
      <TableRowCell>{releaseDate ? formatDate(releaseDate) : '-'}</TableRowCell>
      <TableRowCell>
        <Icon
          name={monitored ? icons.MONITORED : icons.UNMONITORED}
          title={monitored ? 'Monitored' : 'Unmonitored'}
        />
      </TableRowCell>
    </TableRow>
  );
}

export default AudiobookIndexRow;
