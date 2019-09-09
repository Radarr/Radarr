import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { icons, kinds, tooltipPositions } from 'Helpers/Props';
import Icon from 'Components/Icon';
import IconButton from 'Components/Link/IconButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import RelativeDateCellConnector from 'Components/Table/Cells/RelativeDateCellConnector';
import TableRow from 'Components/Table/TableRow';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import Popover from 'Components/Tooltip/Popover';
import TrackQuality from 'Album/TrackQuality';
import HistoryDetailsConnector from 'Activity/History/Details/HistoryDetailsConnector';
import HistoryEventTypeCell from 'Activity/History/HistoryEventTypeCell';
import styles from './ArtistHistoryRow.css';

function getTitle(eventType) {
  switch (eventType) {
    case 'grabbed':
      return 'Grabbed';
    case 'downloadImported':
      return 'Download Completed';
    case 'trackFileImported':
      return 'Track Imported';
    case 'downloadFailed':
      return 'Download Failed';
    case 'trackFileDeleted':
      return 'Track File Deleted';
    case 'trackFileRenamed':
      return 'Track File Renamed';
    case 'trackFileRetagged':
      return 'Track File Tags Updated';
    case 'albumImportIncomplete':
      return 'Album Import Incomplete';
    default:
      return 'Unknown';
  }
}

class ArtistHistoryRow extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isMarkAsFailedModalOpen: false
    };
  }

  //
  // Listeners

  onMarkAsFailedPress = () => {
    this.setState({ isMarkAsFailedModalOpen: true });
  }

  onConfirmMarkAsFailed = () => {
    this.props.onMarkAsFailedPress(this.props.id);
    this.setState({ isMarkAsFailedModalOpen: false });
  }

  onMarkAsFailedModalClose = () => {
    this.setState({ isMarkAsFailedModalOpen: false });
  }

  //
  // Render

  render() {
    const {
      eventType,
      sourceTitle,
      quality,
      qualityCutoffNotMet,
      date,
      data,
      album
    } = this.props;

    const {
      isMarkAsFailedModalOpen
    } = this.state;

    return (
      <TableRow>
        <HistoryEventTypeCell
          eventType={eventType}
          data={data}
        />

        <TableRowCell key={name}>
          {album.title}
        </TableRowCell>

        <TableRowCell>
          {sourceTitle}
        </TableRowCell>

        <TableRowCell>
          <TrackQuality
            quality={quality}
            isCutoffNotMet={qualityCutoffNotMet}
          />
        </TableRowCell>

        <RelativeDateCellConnector
          date={date}
        />

        <TableRowCell className={styles.details}>
          <Popover
            anchor={
              <Icon
                name={icons.INFO}
              />
            }
            title={getTitle(eventType)}
            body={
              <HistoryDetailsConnector
                eventType={eventType}
                sourceTitle={sourceTitle}
                data={data}
              />
            }
            position={tooltipPositions.LEFT}
          />
        </TableRowCell>

        <TableRowCell className={styles.actions}>
          {
            eventType === 'grabbed' &&
              <IconButton
                title="Mark as failed"
                name={icons.REMOVE}
                onPress={this.onMarkAsFailedPress}
              />
          }
        </TableRowCell>

        <ConfirmModal
          isOpen={isMarkAsFailedModalOpen}
          kind={kinds.DANGER}
          title="Mark as Failed"
          message={`Are you sure you want to mark '${sourceTitle}' as failed?`}
          confirmLabel="Mark as Failed"
          onConfirm={this.onConfirmMarkAsFailed}
          onCancel={this.onMarkAsFailedModalClose}
        />
      </TableRow>
    );
  }
}

ArtistHistoryRow.propTypes = {
  id: PropTypes.number.isRequired,
  eventType: PropTypes.string.isRequired,
  sourceTitle: PropTypes.string.isRequired,
  quality: PropTypes.object.isRequired,
  qualityCutoffNotMet: PropTypes.bool.isRequired,
  date: PropTypes.string.isRequired,
  data: PropTypes.object.isRequired,
  fullArtist: PropTypes.bool.isRequired,
  artist: PropTypes.object.isRequired,
  album: PropTypes.object.isRequired,
  onMarkAsFailedPress: PropTypes.func.isRequired
};

export default ArtistHistoryRow;
