import PropTypes from 'prop-types';
import React from 'react';
import episodeEntities from 'Episode/episodeEntities';
import EpisodeTitleLink from 'Episode/EpisodeTitleLink';
import EpisodeStatusConnector from 'Episode/EpisodeStatusConnector';
import EpisodeSearchCellConnector from 'Episode/EpisodeSearchCellConnector';
import TrackFileLanguageConnector from 'TrackFile/TrackFileLanguageConnector';
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
    title,
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

          if (name === 'artist.sortName') {
            return (
              <TableRowCell key={name}>
                <ArtistNameLink
                  nameSlug={artist.nameSlug}
                  artistName={artist.artistName}
                />
              </TableRowCell>
            );
          }

          if (name === 'albumTitle') {
            return (
              <TableRowCell key={name}>
                <EpisodeTitleLink
                  albumId={id}
                  artistId={artist.id}
                  episodeEntity={episodeEntities.WANTED_CUTOFF_UNMET}
                  episodeTitle={title}
                  showOpenArtistButton={true}
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

          if (name === 'language') {
            return (
              <TableRowCell
                key={name}
                className={styles.language}
              >
                <TrackFileLanguageConnector
                  trackFileId={trackFileId}
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
                <EpisodeStatusConnector
                  albumId={id}
                  trackFileId={trackFileId}
                  episodeEntity={episodeEntities.WANTED_CUTOFF_UNMET}
                />
              </TableRowCell>
            );
          }

          if (name === 'actions') {
            return (
              <EpisodeSearchCellConnector
                key={name}
                albumId={id}
                artistId={artist.id}
                episodeTitle={title}
                episodeEntity={episodeEntities.WANTED_CUTOFF_UNMET}
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
  title: PropTypes.string.isRequired,
  isSelected: PropTypes.bool,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  onSelectedChange: PropTypes.func.isRequired
};

export default CutoffUnmetRow;
