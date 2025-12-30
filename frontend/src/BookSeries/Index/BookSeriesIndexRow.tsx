import React from 'react';
import BookSeries from 'BookSeries/BookSeries';
import Icon from 'Components/Icon';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRow from 'Components/Table/TableRow';
import { icons } from 'Helpers/Props';

function BookSeriesIndexRow(props: BookSeries) {
  const { title, description, monitored } = props;

  return (
    <TableRow>
      <TableRowCell>{title}</TableRowCell>
      <TableRowCell>{description || '-'}</TableRowCell>
      <TableRowCell>
        <Icon
          name={monitored ? icons.MONITORED : icons.UNMONITORED}
          title={monitored ? 'Monitored' : 'Unmonitored'}
        />
      </TableRowCell>
    </TableRow>
  );
}

export default BookSeriesIndexRow;
