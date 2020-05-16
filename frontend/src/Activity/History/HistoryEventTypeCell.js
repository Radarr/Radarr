import PropTypes from 'prop-types';
import React from 'react';
import { icons, kinds } from 'Helpers/Props';
import Icon from 'Components/Icon';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import styles from './HistoryEventTypeCell.css';

function getIconName(eventType) {
  switch (eventType) {
    case 'grabbed':
      return icons.DOWNLOADING;
    case 'authorFolderImported':
      return icons.DRIVE;
    case 'bookFileImported':
      return icons.DOWNLOADED;
    case 'downloadFailed':
      return icons.DOWNLOADING;
    case 'bookFileDeleted':
      return icons.DELETE;
    case 'bookFileRenamed':
      return icons.ORGANIZE;
    case 'bookFileRetagged':
      return icons.RETAG;
    case 'bookImportIncomplete':
      return icons.DOWNLOADED;
    case 'downloadImported':
      return icons.DOWNLOADED;
    case 'downloadIgnored':
      return icons.IGNORE;
    default:
      return icons.UNKNOWN;
  }
}

function getIconKind(eventType) {
  switch (eventType) {
    case 'downloadFailed':
      return kinds.DANGER;
    case 'bookImportIncomplete':
      return kinds.WARNING;
    default:
      return kinds.DEFAULT;
  }
}

function getTooltip(eventType, data) {
  switch (eventType) {
    case 'grabbed':
      return `Book grabbed from ${data.indexer} and sent to ${data.downloadClient}`;
    case 'authorFolderImported':
      return 'Book imported from author folder';
    case 'bookFileImported':
      return 'Book downloaded successfully and picked up from download client';
    case 'downloadFailed':
      return 'Book download failed';
    case 'bookFileDeleted':
      return 'Book file deleted';
    case 'bookFileRenamed':
      return 'Book file renamed';
    case 'bookFileRetagged':
      return 'Book file tags updated';
    case 'bookImportIncomplete':
      return 'Files downloaded but not all could be imported';
    case 'downloadImported':
      return 'Download completed and successfully imported';
    case 'downloadIgnored':
      return 'Book Download Ignored';
    default:
      return 'Unknown event';
  }
}

function HistoryEventTypeCell({ eventType, data }) {
  const iconName = getIconName(eventType);
  const iconKind = getIconKind(eventType);
  const tooltip = getTooltip(eventType, data);

  return (
    <TableRowCell
      className={styles.cell}
      title={tooltip}
    >
      <Icon
        name={iconName}
        kind={iconKind}
      />
    </TableRowCell>
  );
}

HistoryEventTypeCell.propTypes = {
  eventType: PropTypes.string.isRequired,
  data: PropTypes.object
};

HistoryEventTypeCell.defaultProps = {
  data: {}
};

export default HistoryEventTypeCell;
