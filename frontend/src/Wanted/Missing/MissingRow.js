import PropTypes from 'prop-types';
import React from 'react';
import RelativeDateCellConnector from 'Components/Table/Cells/RelativeDateCellConnector';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';
import TableRow from 'Components/Table/TableRow';
import movieEntities from 'Movie/movieEntities';
import MovieSearchCell from 'Movie/MovieSearchCell';
import MovieStatusConnector from 'Movie/MovieStatusConnector';
import MovieTitleLink from 'Movie/MovieTitleLink';
import styles from './MissingRow.css';

function MissingRow(props) {
  const {
    id,
    movieFileId,
    year,
    title,
    titleSlug,
    inCinemas,
    digitalRelease,
    physicalRelease,
    isSelected,
    columns,
    onSelectedChange
  } = props;

  if (!title) {
    return null;
  }

  return (
    <TableRow>
      <TableSelectCell
        id={id}
        isSelected={isSelected}
        onSelectedChange={onSelectedChange}
      />

      {
        columns.map((column) => {
          const {
            name,
            isVisible
          } = column;

          if (!isVisible) {
            return null;
          }

          if (name === 'movieMetadata.sortTitle') {
            return (
              <TableRowCell key={name}>
                <MovieTitleLink
                  titleSlug={titleSlug}
                  title={title}
                />
              </TableRowCell>
            );
          }

          if (name === 'movieMetadata.year') {
            return (
              <TableRowCell key={name}>
                {year}
              </TableRowCell>
            );
          }

          if (name === 'movieMetadata.inCinemas') {
            return (
              <RelativeDateCellConnector
                key={name}
                className={styles[name]}
                date={inCinemas}
              />
            );
          }

          if (name === 'movieMetadata.digitalRelease') {
            return (
              <RelativeDateCellConnector
                key={name}
                className={styles[name]}
                date={digitalRelease}
              />
            );
          }

          if (name === 'movieMetadata.physicalRelease') {
            return (
              <RelativeDateCellConnector
                key={name}
                className={styles[name]}
                date={physicalRelease}
              />
            );
          }

          if (name === 'status') {
            return (
              <TableRowCell
                key={name}
                className={styles.status}
              >
                <MovieStatusConnector
                  movieId={id}
                  movieFileId={movieFileId}
                  movieEntity={movieEntities.WANTED_MISSING}
                />
              </TableRowCell>
            );
          }

          if (name === 'actions') {
            return (
              <MovieSearchCell
                key={name}
                movieId={id}
                movieTitle={title}
                movieEntity={movieEntities.WANTED_MISSING}
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
  movieFileId: PropTypes.number,
  title: PropTypes.string.isRequired,
  year: PropTypes.number.isRequired,
  titleSlug: PropTypes.string.isRequired,
  inCinemas: PropTypes.string,
  digitalRelease: PropTypes.string,
  physicalRelease: PropTypes.string,
  isSelected: PropTypes.bool,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  onSelectedChange: PropTypes.func.isRequired
};

export default MissingRow;
