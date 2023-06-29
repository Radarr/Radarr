import PropTypes from 'prop-types';
import React from 'react';
import RelativeDateCellConnector from 'Components/Table/Cells/RelativeDateCellConnector';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';
import TableRow from 'Components/Table/TableRow';
import MovieTitleLink from 'Movie/MovieTitleLink';

function MissingRow(props) {
  const {
    id,
    title,
    movie,
    isSelected,
    columns,
    onSelectedChange
  } = props;

  return (
    <TableRow>
      <TableSelectCell
        id={id}
        isSelected={isSelected}
        onSelectedChange={onSelectedChange}
      />

      {columns.map((column) => {
        const {
          name,
          isVisible
        } = column;

        if (!isVisible) {
          return null;
        }

        if (name === 'sortTitle') {
          return (
            <TableRowCell key={name}>
              <MovieTitleLink title={title} titleSlug={movie.titleSlug} />
            </TableRowCell>
          );
        }

        if (name === 'inCinemas') {
          return (
            <RelativeDateCellConnector
              key={name}
              date={movie.inCinemas}
            />
          );
        }

        return null;
      })
      }
    </TableRow>
  );
}

MissingRow.propTypes = {
  id: PropTypes.number.isRequired,
  title: PropTypes.string.isRequired,
  movie: PropTypes.object.isRequired,
  isSelected: PropTypes.bool,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  onSelectedChange: PropTypes.func.isRequired
};

export default MissingRow;
