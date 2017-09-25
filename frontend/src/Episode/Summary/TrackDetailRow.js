import PropTypes from 'prop-types';
import React, { Component } from 'react';
import TableRow from 'Components/Table/TableRow';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import formatTimeSpan from 'Utilities/Date/formatTimeSpan';
import MediaInfoConnector from 'TrackFile/MediaInfoConnector';
import * as mediaInfoTypes from 'TrackFile/mediaInfoTypes';
import EpisodeStatusConnector from 'Episode/EpisodeStatusConnector';

import styles from './TrackDetailRow.css';

class TrackDetailRow extends Component {

  //
  // Lifecycle

  //
  // Listeners

  //
  // Render

  render() {
    const {
      id,
      title,
      trackNumber,
      duration,
      columns,
      trackFileId
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

            if (name === 'trackNumber') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.trackNumber}
                >
                  {trackNumber}
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

            if (name === 'duration') {
              return (
                <TableRowCell key={name}>
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
                    episodeId={id}
                    trackFileId={trackFileId}
                  />
                </TableRowCell>
              );
            }

            return null;
          })
        }
      </TableRow>
    );
  }
}

TrackDetailRow.propTypes = {
  id: PropTypes.number.isRequired,
  title: PropTypes.string.isRequired,
  duration: PropTypes.number.isRequired,
  trackFileId: PropTypes.number.isRequired,
  columns: PropTypes.arrayOf(PropTypes.object).isRequired,
  trackNumber: PropTypes.number.isRequired
};

export default TrackDetailRow;
