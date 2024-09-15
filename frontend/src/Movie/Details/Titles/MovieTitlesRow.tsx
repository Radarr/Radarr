import React from 'react';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRow from 'Components/Table/TableRow';
import titleCase from 'Utilities/String/titleCase';

interface MovieTitlesRowProps {
  title: string;
  sourceType: string;
}

function MovieTitlesRow({ title, sourceType }: MovieTitlesRowProps) {
  return (
    <TableRow>
      <TableRowCell>{title}</TableRowCell>

      <TableRowCell>{titleCase(sourceType)}</TableRowCell>
    </TableRow>
  );
}

export default MovieTitlesRow;
