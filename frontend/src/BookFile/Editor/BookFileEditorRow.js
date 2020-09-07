import PropTypes from 'prop-types';
import React from 'react';
import BookQuality from 'Book/BookQuality';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';
import TableRow from 'Components/Table/TableRow';

function BookFileEditorRow(props) {
  const {
    id,
    path,
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
        {path}
      </TableRowCell>

      <TableRowCell>
        <BookQuality
          quality={quality}
        />
      </TableRowCell>
    </TableRow>
  );
}

BookFileEditorRow.propTypes = {
  id: PropTypes.number.isRequired,
  path: PropTypes.string.isRequired,
  quality: PropTypes.object.isRequired,
  isSelected: PropTypes.bool,
  onSelectedChange: PropTypes.func.isRequired
};

export default BookFileEditorRow;
