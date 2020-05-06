import PropTypes from 'prop-types';
import React, { Component } from 'react';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import RelativeDateCellConnector from 'Components/Table/Cells/RelativeDateCellConnector';
import { kinds, sizes } from 'Helpers/Props';
import TableRow from 'Components/Table/TableRow';
import Label from 'Components/Label';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import AlbumSearchCellConnector from 'Album/AlbumSearchCellConnector';
import AlbumTitleLink from 'Album/AlbumTitleLink';
import StarRating from 'Components/StarRating';
import styles from './AlbumRow.css';

function getTrackCountKind(monitored, trackFileCount, trackCount) {
  if (trackFileCount === trackCount && trackCount > 0) {
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
      isDetailsModalOpen: false,
      isEditAlbumModalOpen: false
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

  onEditAlbumPress = () => {
    this.setState({ isEditAlbumModalOpen: true });
  }

  onEditAlbumModalClose = () => {
    this.setState({ isEditAlbumModalOpen: false });
  }

  onMonitorAlbumPress = (monitored, options) => {
    this.props.onMonitorAlbumPress(this.props.id, monitored, options);
  }

  //
  // Render

  render() {
    const {
      id,
      authorId,
      monitored,
      statistics,
      releaseDate,
      title,
      position,
      ratings,
      disambiguation,
      isSaving,
      artistMonitored,
      titleSlug,
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
                  <AlbumTitleLink
                    titleSlug={titleSlug}
                    title={title}
                    disambiguation={disambiguation}
                  />
                </TableRowCell>
              );
            }

            if (name === 'position') {
              return (
                <TableRowCell
                  key={name}
                  className={styles.title}
                >
                  {position || ''}
                </TableRowCell>
              );
            }

            if (name === 'rating') {
              return (
                <TableRowCell key={name}>
                  {
                    <StarRating
                      rating={ratings.value}
                      votes={ratings.votes}
                    />
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
                    kind={getTrackCountKind(monitored, trackFileCount, trackCount)}
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
                <AlbumSearchCellConnector
                  key={name}
                  bookId={id}
                  authorId={authorId}
                  albumTitle={title}
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
  authorId: PropTypes.number.isRequired,
  monitored: PropTypes.bool.isRequired,
  releaseDate: PropTypes.string,
  title: PropTypes.string.isRequired,
  position: PropTypes.string,
  ratings: PropTypes.object.isRequired,
  disambiguation: PropTypes.string,
  titleSlug: PropTypes.string.isRequired,
  isSaving: PropTypes.bool,
  artistMonitored: PropTypes.bool.isRequired,
  statistics: PropTypes.object.isRequired,
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
