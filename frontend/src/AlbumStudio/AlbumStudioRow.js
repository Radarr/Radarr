import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { icons } from 'Helpers/Props';
import Icon from 'Components/Icon';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import TableRow from 'Components/Table/TableRow';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableSelectCell from 'Components/Table/Cells/TableSelectCell';
import ArtistNameLink from 'Artist/ArtistNameLink';
import AlbumStudioAlbum from './AlbumStudioAlbum';
import styles from './AlbumStudioRow.css';

class AlbumStudioRow extends Component {

  //
  // Render

  render() {
    const {
      artistId,
      status,
      foreignArtistId,
      artistName,
      monitored,
      albums,
      isSaving,
      isSelected,
      onSelectedChange,
      onArtistMonitoredPress,
      onAlbumMonitoredPress
    } = this.props;

    return (
      <TableRow>
        <TableSelectCell
          id={artistId}
          isSelected={isSelected}
          onSelectedChange={onSelectedChange}
        />

        <TableRowCell className={styles.status}>
          <Icon
            className={styles.statusIcon}
            name={status === 'ended' ? icons.ARTIST_ENDED : icons.ARTIST_CONTINUING}
            title={status === 'ended' ? 'Ended' : 'Continuing'}
          />
        </TableRowCell>

        <TableRowCell className={styles.title}>
          <ArtistNameLink
            foreignArtistId={foreignArtistId}
            artistName={artistName}
          />
        </TableRowCell>

        <TableRowCell className={styles.monitored}>
          <MonitorToggleButton
            monitored={monitored}
            isSaving={isSaving}
            onPress={onArtistMonitoredPress}
          />
        </TableRowCell>

        <TableRowCell className={styles.albums}>
          {
            albums.map((album) => {
              return (
                <AlbumStudioAlbum
                  key={album.id}
                  {...album}
                  onAlbumMonitoredPress={onAlbumMonitoredPress}
                />
              );
            })
          }
        </TableRowCell>
      </TableRow>
    );
  }
}

AlbumStudioRow.propTypes = {
  artistId: PropTypes.number.isRequired,
  status: PropTypes.string.isRequired,
  foreignArtistId: PropTypes.string.isRequired,
  artistName: PropTypes.string.isRequired,
  monitored: PropTypes.bool.isRequired,
  albums: PropTypes.arrayOf(PropTypes.object).isRequired,
  isSaving: PropTypes.bool.isRequired,
  isSelected: PropTypes.bool,
  onSelectedChange: PropTypes.func.isRequired,
  onArtistMonitoredPress: PropTypes.func.isRequired,
  onAlbumMonitoredPress: PropTypes.func.isRequired
};

AlbumStudioRow.defaultProps = {
  isSaving: false
};

export default AlbumStudioRow;
