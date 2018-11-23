import PropTypes from 'prop-types';
import React from 'react';
import Label from 'Components/Label';
import RelativeDateCellConnector from 'Components/Table/Cells/RelativeDateCellConnector';
import TableRow from 'Components/Table/TableRow';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';
import MovieQuality from 'Movie/MovieQuality';

function MovieFileEditorRow(props) {
  const {
    id,
    relativePath,
    airDateUtc,
    language,
    quality,
    isSelected,
    onSelectedChange
  } = props;

  return (
    <TableRow>
      <TableSelectCell
        id={id}
        isSelected={isSelected}
        onSelectedChange={onSelectedChange}
      />

      <TableRowCell>
        {relativePath}
      </TableRowCell>

      <RelativeDateCellConnector
        date={airDateUtc}
      />

      <TableRowCell>
        <Label>
          {language.name}
        </Label>
      </TableRowCell>

      <TableRowCell>
        <MovieQuality
          quality={quality}
        />
      </TableRowCell>
    </TableRow>
  );
}

MovieFileEditorRow.propTypes = {
  id: PropTypes.number.isRequired,
  relativePath: PropTypes.string.isRequired,
  airDateUtc: PropTypes.string.isRequired,
  language: PropTypes.object.isRequired,
  quality: PropTypes.object.isRequired,
  isSelected: PropTypes.bool,
  onSelectedChange: PropTypes.func.isRequired
};

export default MovieFileEditorRow;
