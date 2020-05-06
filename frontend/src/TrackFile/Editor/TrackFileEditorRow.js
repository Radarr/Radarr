import PropTypes from 'prop-types';
import React from 'react';
import TableRow from 'Components/Table/TableRow';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';
import TrackQuality from 'Album/TrackQuality';

function TrackFileEditorRow(props) {
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
        <TrackQuality
          quality={quality}
        />
      </TableRowCell>
    </TableRow>
  );
}

TrackFileEditorRow.propTypes = {
  id: PropTypes.number.isRequired,
  path: PropTypes.string.isRequired,
  quality: PropTypes.object.isRequired,
  isSelected: PropTypes.bool,
  onSelectedChange: PropTypes.func.isRequired
};

export default TrackFileEditorRow;
