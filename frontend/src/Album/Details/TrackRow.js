import PropTypes from 'prop-types';
import React, { Component } from 'react';
import TableRow from 'Components/Table/TableRow';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import formatTimeSpan from 'Utilities/Date/formatTimeSpan';
import EpisodeStatusConnector from 'Album/EpisodeStatusConnector';
import MediaInfoConnector from 'TrackFile/MediaInfoConnector';
import TrackActionsCell from './TrackActionsCell';
import * as mediaInfoTypes from 'TrackFile/mediaInfoTypes';

import styles from './TrackRow.css';

class TrackRow extends Component {

  //
  // Render

  render() {
    const {
      id,
      albumId,
      mediumNumber,
      trackFileId,
      absoluteTrackNumber,
      title,
      duration,
      trackFilePath,
      trackFileRelativePath,
      columns,
      deleteTrackFile
    } = this.props;

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

            if (name === 'medium') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.trackNumber}
                >
                  {mediumNumber}
                </TableRowCell>
              );
            }

            if (name === 'absoluteTrackNumber') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.trackNumber}
                >
                  {absoluteTrackNumber}
                </TableRowCell>
              );
            }

            if (name === 'title') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.title}
                >
                  {title}
                </TableRowCell>
              );
            }

            if (name === 'path') {
              return (
                <TableRowCell key={name}>
                  {
                    trackFilePath
                  }
                </TableRowCell>
              );
            }

            if (name === 'relativePath') {
              return (
                <TableRowCell key={name}>
                  {
                    trackFileRelativePath
                  }
                </TableRowCell>
              );
            }

            if (name === 'duration') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.duration}
                >
                  {
                    formatTimeSpan(duration)
                  }
                </TableRowCell>
              );
            }

            if (name === 'audioInfo') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.audio}
                >
                  <MediaInfoConnector
                    type={mediaInfoTypes.AUDIO}
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
                  />
                </TableRowCell>
              );
            }

            if (name === 'actions') {
              return (
                <TrackActionsCell
                  key={name}
                  albumId={albumId}
                  id={id}
                  trackFilePath={trackFilePath}
                  trackFileId={trackFileId}
                  deleteTrackFile={deleteTrackFile}
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

TrackRow.propTypes = {
  deleteTrackFile: PropTypes.func.isRequired,
  id: PropTypes.number.isRequired,
  albumId: PropTypes.number.isRequired,
  trackFileId: PropTypes.number,
  mediumNumber: PropTypes.number.isRequired,
  trackNumber: PropTypes.string.isRequired,
  absoluteTrackNumber: PropTypes.number,
  title: PropTypes.string.isRequired,
  duration: PropTypes.number.isRequired,
  isSaving: PropTypes.bool,
  trackFilePath: PropTypes.string,
  trackFileRelativePath: PropTypes.string,
  mediaInfo: PropTypes.object,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired
};

export default TrackRow;
