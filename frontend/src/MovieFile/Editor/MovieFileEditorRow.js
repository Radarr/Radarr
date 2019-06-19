import PropTypes from 'prop-types';
import React from 'react';
import Label from 'Components/Label';
import RelativeDateCellConnector from 'Components/Table/Cells/RelativeDateCellConnector';
import TableRow from 'Components/Table/TableRow';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';
import MovieQuality from 'Movie/MovieQuality';
import MovieLanguage from 'Movie/MovieLanguage';
import * as mediaInfoTypes from 'MovieFile/mediaInfoTypes';
import MediaInfoConnector from 'MovieFile/MediaInfoConnector';
import styles from './MovieFileEditorRow.css';

function MovieFileEditorRow(props) {
  const {
    id,
    relativePath,
    size,
    quality,
    languages,
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

      <TableRowCell>
        <MediaInfoConnector
          movieFileId={id}
          type={mediaInfoTypes.VIDEO}
        />
        <MediaInfoConnector
          movieFileId={id}
          type={mediaInfoTypes.AUDIO}
        />
      </TableRowCell>

      <TableRowCell className={styles.language}>
        <MovieLanguage
          languages={languages}
        />
      </TableRowCell>

      <TableRowCell className={styles.quality}>
        <MovieQuality
          quality={quality}
          size={size}
        />
      </TableRowCell>
    </TableRow>
  );
}

MovieFileEditorRow.propTypes = {
  id: PropTypes.number.isRequired,
  size: PropTypes.number.isRequired,
  relativePath: PropTypes.string.isRequired,
  quality: PropTypes.object.isRequired,
  languages: PropTypes.arrayOf(PropTypes.object),
  mediaInfo: PropTypes.object.isRequired,
  isSelected: PropTypes.bool,
  onSelectedChange: PropTypes.func.isRequired
};

export default MovieFileEditorRow;
