import React from 'react';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRow from 'Components/Table/TableRow';
import languageToFlag from 'Utilities/String/languageToFlag';
import titleCase from 'Utilities/String/titleCase';

interface MovieTitlesRowProps {
  title: string;
  language?: string;
  sourceType: string;
}

function MovieTitlesRow({ title, language, sourceType }: MovieTitlesRowProps) {
  return (
    <TableRow>
      <TableRowCell>{title}</TableRowCell>

      <TableRowCell>
        {language && language !== 'Unknown'
          ? `${languageToFlag(language)} ${language}`
          : '-'}
      </TableRowCell>

      <TableRowCell>{titleCase(sourceType)}</TableRowCell>
    </TableRow>
  );
}

export default MovieTitlesRow;
