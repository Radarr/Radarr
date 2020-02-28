import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { icons } from 'Helpers/Props';
import Icon from 'Components/Icon';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import VirtualTableRowCell from 'Components/Table/Cells/VirtualTableRowCell';
import VirtualTableSelectCell from 'Components/Table/Cells/VirtualTableSelectCell';
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
      <>
        <VirtualTableSelectCell
          className={styles.selectCell}
          id={artistId}
          isSelected={isSelected}
          onSelectedChange={onSelectedChange}
          isDisabled={false}
        />

        <VirtualTableRowCell className={styles.monitored}>
          <MonitorToggleButton
            monitored={monitored}
            size={14}
            isSaving={isSaving}
            onPress={onArtistMonitoredPress}
          />
        </VirtualTableRowCell>

        <VirtualTableRowCell className={styles.status}>
          <Icon
            className={styles.statusIcon}
            name={status === 'ended' ? icons.ARTIST_ENDED : icons.ARTIST_CONTINUING}
            title={status === 'ended' ? 'Ended' : 'Continuing'}
          />
        </VirtualTableRowCell>

        <VirtualTableRowCell className={styles.title}>
          <ArtistNameLink
            foreignArtistId={foreignArtistId}
            artistName={artistName}
          />
        </VirtualTableRowCell>

        <VirtualTableRowCell className={styles.albums}>
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
        </VirtualTableRowCell>
      </>
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
