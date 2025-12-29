import React from 'react';
import Author from 'Author/Author';
import Icon from 'Components/Icon';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRow from 'Components/Table/TableRow';
import { icons } from 'Helpers/Props';

function AuthorIndexRow(props: Author) {
  const { name, path, monitored } = props;

  return (
    <TableRow>
      <TableRowCell>{name}</TableRowCell>
      <TableRowCell>{path || '-'}</TableRowCell>
      <TableRowCell>
        <Icon
          name={monitored ? icons.MONITORED : icons.UNMONITORED}
          title={monitored ? 'Monitored' : 'Unmonitored'}
        />
      </TableRowCell>
    </TableRow>
  );
}

export default AuthorIndexRow;
