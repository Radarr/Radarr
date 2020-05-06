import PropTypes from 'prop-types';
import React from 'react';
import albumEntities from 'Album/albumEntities';
import AlbumTitleLink from 'Album/AlbumTitleLink';
import EpisodeStatusConnector from 'Album/EpisodeStatusConnector';
import AlbumSearchCellConnector from 'Album/AlbumSearchCellConnector';
import ArtistNameLink from 'Artist/ArtistNameLink';
import RelativeDateCellConnector from 'Components/Table/Cells/RelativeDateCellConnector';
import TableRow from 'Components/Table/TableRow';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';
import styles from './CutoffUnmetRow.css';

function CutoffUnmetRow(props) {
  const {
    id,
    trackFileId,
    artist,
    releaseDate,
    titleSlug,
    title,
    disambiguation,
    isSelected,
    columns,
    onSelectedChange
  } = props;

  if (!artist) {
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

          if (name === 'authors.sortName') {
            return (
              <TableRowCell key={name}>
                <ArtistNameLink
                  titleSlug={artist.titleSlug}
                  artistName={artist.artistName}
                />
              </TableRowCell>
            );
          }

          if (name === 'books.title') {
            return (
              <TableRowCell key={name}>
                <AlbumTitleLink
                  titleSlug={titleSlug}
                  title={title}
                  disambiguation={disambiguation}
                />
              </TableRowCell>
            );
          }

          if (name === 'releaseDate') {
            return (
              <RelativeDateCellConnector
                key={name}
                date={releaseDate}
              />
            );
          }

          if (name === 'status') {
            return (
              <TableRowCell
                key={name}
                className={styles.status}
              >
                <EpisodeStatusConnector
                  bookId={id}
                  trackFileId={trackFileId}
                  albumEntity={albumEntities.WANTED_CUTOFF_UNMET}
                />
              </TableRowCell>
            );
          }

          if (name === 'actions') {
            return (
              <AlbumSearchCellConnector
                key={name}
                bookId={id}
                authorId={artist.id}
                albumTitle={title}
                albumEntity={albumEntities.WANTED_CUTOFF_UNMET}
                showOpenArtistButton={true}
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
  trackFileId: PropTypes.number,
  artist: PropTypes.object.isRequired,
  releaseDate: PropTypes.string.isRequired,
  titleSlug: PropTypes.string.isRequired,
  title: PropTypes.string.isRequired,
  disambiguation: PropTypes.string,
  isSelected: PropTypes.bool,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  onSelectedChange: PropTypes.func.isRequired
};

export default CutoffUnmetRow;
