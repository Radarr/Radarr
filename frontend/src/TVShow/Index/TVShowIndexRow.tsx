import React from 'react';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRow from 'Components/Table/TableRow';
import { icons } from 'Helpers/Props';
import TVShow from 'TVShow/TVShow';

function TVShowIndexRow(props: TVShow) {
  const { id, title, network, status, year, monitored } = props;

  return (
    <TableRow>
      <TableRowCell>
        <Link to={`/tvshow/${id}`}>{title}</Link>
      </TableRowCell>
      <TableRowCell>{network || '-'}</TableRowCell>
      <TableRowCell>{status}</TableRowCell>
      <TableRowCell>{year}</TableRowCell>
      <TableRowCell>
        <Icon
          name={monitored ? icons.MONITORED : icons.UNMONITORED}
          title={monitored ? 'Monitored' : 'Unmonitored'}
        />
      </TableRowCell>
    </TableRow>
  );
}

export default TVShowIndexRow;
