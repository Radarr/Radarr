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
      nameSlug,
      artistName,
      monitored,
      albums,
      isSaving,
      isSelected,
      onSelectedChange,
      onSeriesMonitoredPress,
      onSeasonMonitoredPress
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
            name={status === 'ended' ? icons.SERIES_ENDED : icons.SERIES_CONTINUING}
            title={status === 'ended' ? 'Ended' : 'Continuing'}

          />
        </TableRowCell>

        <TableRowCell className={styles.title}>
          <ArtistNameLink
            nameSlug={nameSlug}
            artistName={artistName}
          />
        </TableRowCell>

        <TableRowCell className={styles.monitored}>
          <MonitorToggleButton
            monitored={monitored}
            isSaving={isSaving}
            onPress={onSeriesMonitoredPress}
          />
        </TableRowCell>

        <TableRowCell className={styles.seasons}>
          {
            albums.map((season) => {
              return (
                <AlbumStudioAlbum
                  key={season.id}
                  {...season}
                  onSeasonMonitoredPress={onSeasonMonitoredPress}
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
  nameSlug: PropTypes.string.isRequired,
  artistName: PropTypes.string.isRequired,
  monitored: PropTypes.bool.isRequired,
  albums: PropTypes.arrayOf(PropTypes.object).isRequired,
  isSaving: PropTypes.bool.isRequired,
  isSelected: PropTypes.bool,
  onSelectedChange: PropTypes.func.isRequired,
  onSeriesMonitoredPress: PropTypes.func.isRequired,
  onSeasonMonitoredPress: PropTypes.func.isRequired
};

AlbumStudioRow.defaultProps = {
  isSaving: false
};

export default AlbumStudioRow;
