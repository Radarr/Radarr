import PropTypes from 'prop-types';
import React from 'react';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';
import TableRow from 'Components/Table/TableRow';
import movieEntities from 'Movie/movieEntities';
import MovieSearchCellConnector from 'Movie/MovieSearchCellConnector';
import MovieStatusConnector from 'Movie/MovieStatusConnector';
import MovieTitleLink from 'Movie/MovieTitleLink';
import MovieFileLanguageConnector from 'MovieFile/MovieFileLanguageConnector';
import styles from './CutoffUnmetRow.css';

function CutoffUnmetRow(props) {
  const {
    id,
    movieFileId,
    year,
    title,
    titleSlug,
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

          if (name === 'languages') {
            return (
              <TableRowCell
                key={name}
                className={styles.languages}
              >
                <MovieFileLanguageConnector
                  movieFileId={movieFileId}
                />
              </TableRowCell>
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
                  movieEntity={movieEntities.WANTED_CUTOFF_UNMET}
                />
              </TableRowCell>
            );
          }

          if (name === 'actions') {
            return (
              <MovieSearchCellConnector
                key={name}
                movieId={id}
                movieTitle={title}
                movieEntity={movieEntities.WANTED_CUTOFF_UNMET}
              />
            );
          }

          return null;
        })
      }
    </TableRow>
  );
}

CutoffUnmetRow.propTypes = {
  id: PropTypes.number.isRequired,
  movieFileId: PropTypes.number,
  title: PropTypes.string.isRequired,
  year: PropTypes.number.isRequired,
  titleSlug: PropTypes.string.isRequired,
  isSelected: PropTypes.bool,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  onSelectedChange: PropTypes.func.isRequired
};

export default CutoffUnmetRow;
