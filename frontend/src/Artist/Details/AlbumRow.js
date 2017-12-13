import PropTypes from 'prop-types';
import React, { Component } from 'react';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import RelativeDateCellConnector from 'Components/Table/Cells/RelativeDateCellConnector';
import { kinds, sizes } from 'Helpers/Props';
import TableRow from 'Components/Table/TableRow';
import Label from 'Components/Label';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import formatTimeSpan from 'Utilities/Date/formatTimeSpan';
import EpisodeSearchCellConnector from 'Album/EpisodeSearchCellConnector';
import EpisodeTitleLink from 'Album/EpisodeTitleLink';

import styles from './AlbumRow.css';

function getEpisodeCountKind(monitored, trackFileCount, episodeCount) {
  if (trackFileCount === episodeCount && episodeCount > 0) {
    return kinds.SUCCESS;
  }

  if (!monitored) {
    return kinds.WARNING;
  }

  return kinds.DANGER;
}

class AlbumRow extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isDetailsModalOpen: false
    };
  }

  //
  // Listeners

  onManualSearchPress = () => {
    this.setState({ isDetailsModalOpen: true });
  }

  onDetailsModalClose = () => {
    this.setState({ isDetailsModalOpen: false });
  }

  onMonitorAlbumPress = (monitored, options) => {
    this.props.onMonitorAlbumPress(this.props.id, monitored, options);
  }

  //
  // Render

  render() {
    const {
      id,
      artistId,
      monitored,
      statistics,
      duration,
      releaseDate,
      mediumCount,
      title,
      isSaving,
      artistMonitored,
      columns
    } = this.props;

    const {
      trackCount,
      trackFileCount,
      totalTrackCount
    } = statistics;

    return (
      <TableRow>
        {
          columns.map((column) => {
            const {
              name,
              isVisible
            } = column;

            if (!isVisible) {
              return null;
            }

            if (name === 'monitored') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.monitored}
                >
                  <MonitorToggleButton
                    monitored={monitored}
                    isDisabled={!artistMonitored}
                    isSaving={isSaving}
                    onPress={this.onMonitorAlbumPress}
                  />
                </TableRowCell>
              );
            }

            if (name === 'title') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.title}
                >
                  <EpisodeTitleLink
                    albumId={id}
                    artistId={artistId}
                    episodeTitle={title}
                    showOpenArtistButton={false}
                  />
                </TableRowCell>
              );
            }

            if (name === 'mediumCount') {
              return (
                <TableRowCell key={name}>
                  {
                    mediumCount
                  }
                </TableRowCell>
              );
            }

            if (name === 'trackCount') {
              return (
                <TableRowCell key={name}>
                  {
                    statistics.totalTrackCount
                  }
                </TableRowCell>
              );
            }

            if (name === 'duration') {
              return (
                <TableRowCell key={name}>
                  {
                    formatTimeSpan(duration)
                  }
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
                  <Label
                    title={`${totalTrackCount} tracks total. ${trackFileCount} tracks with files.`}
                    kind={getEpisodeCountKind(monitored, trackFileCount, trackCount)}
                    size={sizes.MEDIUM}
                  >
                    {
                      <span>{trackFileCount} / {trackCount}</span>
                    }
                  </Label>
                </TableRowCell>
              );
            }

            if (name === 'actions') {
              return (
                <EpisodeSearchCellConnector
                  key={name}
                  albumId={id}
                  artistId={artistId}
                  episodeTitle={title}
                />
              );
            }

            return null;
          })
        }
      </TableRow>
    );
  }
}

AlbumRow.propTypes = {
  id: PropTypes.number.isRequired,
  artistId: PropTypes.number.isRequired,
  monitored: PropTypes.bool.isRequired,
  releaseDate: PropTypes.string.isRequired,
  mediumCount: PropTypes.number.isRequired,
  duration: PropTypes.number.isRequired,
  title: PropTypes.string.isRequired,
  isSaving: PropTypes.bool,
  unverifiedSceneNumbering: PropTypes.bool,
  artistMonitored: PropTypes.bool.isRequired,
  statistics: PropTypes.object.isRequired,
  mediaInfo: PropTypes.object,
  alternateTitles: PropTypes.arrayOf(PropTypes.object).isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  onMonitorAlbumPress: PropTypes.func.isRequired
};

AlbumRow.defaultProps = {
  statistics: {
    trackCount: 0,
    trackFileCount: 0
  }
};

export default AlbumRow;
