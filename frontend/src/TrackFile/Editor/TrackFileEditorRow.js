import PropTypes from 'prop-types';
import React from 'react';
import padNumber from 'Utilities/Number/padNumber';
import Label from 'Components/Label';
import RelativeDateCellConnector from 'Components/Table/Cells/RelativeDateCellConnector';
import TableRow from 'Components/Table/TableRow';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';
import EpisodeQuality from 'Album/EpisodeQuality';
import styles from './TrackFileEditorRow';

function TrackFileEditorRow(props) {
  const {
    id,
    trackNumber,
    relativePath,
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
        {padNumber(trackNumber, 2)}
      </TableRowCell>

      <TableRowCell>
        {relativePath}
      </TableRowCell>

      <TableRowCell>
        <Label>
          {language.name}
        </Label>
      </TableRowCell>

      <TableRowCell>
        <EpisodeQuality
          quality={quality}
        />
      </TableRowCell>
    </TableRow>
  );
}

TrackFileEditorRow.propTypes = {
  id: PropTypes.number.isRequired,
  trackNumber: PropTypes.string.isRequired,
  relativePath: PropTypes.string.isRequired,
  language: PropTypes.object.isRequired,
  quality: PropTypes.object.isRequired,
  isSelected: PropTypes.bool,
  onSelectedChange: PropTypes.func.isRequired
};

export default TrackFileEditorRow;
