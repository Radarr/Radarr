import React from 'react';
import Book from 'Book/Book';
import Icon from 'Components/Icon';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRow from 'Components/Table/TableRow';
import { icons } from 'Helpers/Props';
import formatDate from 'Utilities/Date/formatDate';

function BookIndexRow(props: Book) {
  const { title, publisher, releaseDate, monitored } = props;

  return (
    <TableRow>
      <TableRowCell>{title}</TableRowCell>
      <TableRowCell>-</TableRowCell>
      <TableRowCell>{publisher || '-'}</TableRowCell>
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

export default BookIndexRow;
