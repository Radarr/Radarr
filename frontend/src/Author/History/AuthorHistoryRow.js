import PropTypes from 'prop-types';
import React, { Component } from 'react';
import HistoryDetailsConnector from 'Activity/History/Details/HistoryDetailsConnector';
import HistoryEventTypeCell from 'Activity/History/HistoryEventTypeCell';
import BookQuality from 'Book/BookQuality';
import Icon from 'Components/Icon';
import IconButton from 'Components/Link/IconButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import RelativeDateCellConnector from 'Components/Table/Cells/RelativeDateCellConnector';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import TableRow from 'Components/Table/TableRow';
import Popover from 'Components/Tooltip/Popover';
import { icons, kinds, tooltipPositions } from 'Helpers/Props';
import styles from './AuthorHistoryRow.css';

function getTitle(eventType) {
  switch (eventType) {
    case 'grabbed':
      return 'Grabbed';
    case 'downloadImported':
      return 'Download Completed';
    case 'bookFileImported':
      return 'Book Imported';
    case 'downloadFailed':
      return 'Download Failed';
    case 'bookFileDeleted':
      return 'Book File Deleted';
    case 'bookFileRenamed':
      return 'Book File Renamed';
    case 'bookFileRetagged':
      return 'Book File Tags Updated';
    case 'bookImportIncomplete':
      return 'Book Import Incomplete';
    default:
      return 'Unknown';
  }
}

class AuthorHistoryRow extends Component {

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
      book
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
          {book.title}
        </TableRowCell>

        <TableRowCell>
          {sourceTitle}
        </TableRowCell>

        <TableRowCell>
          <BookQuality
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

AuthorHistoryRow.propTypes = {
  id: PropTypes.number.isRequired,
  eventType: PropTypes.string.isRequired,
  sourceTitle: PropTypes.string.isRequired,
  quality: PropTypes.object.isRequired,
  qualityCutoffNotMet: PropTypes.bool.isRequired,
  date: PropTypes.string.isRequired,
  data: PropTypes.object.isRequired,
  fullAuthor: PropTypes.bool.isRequired,
  author: PropTypes.object.isRequired,
  book: PropTypes.object.isRequired,
  onMarkAsFailedPress: PropTypes.func.isRequired
};

export default AuthorHistoryRow;
