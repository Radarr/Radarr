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
    case 'seriesFolderImported':
      return icons.DRIVE;
    case 'downloadFolderImported':
      return icons.DOWNLOADED;
    case 'downloadFailed':
      return icons.DOWNLOADING;
    case 'trackFileDeleted':
      return icons.DELETE;
    case 'trackFileRenamed':
      return icons.ORGANIZE;
    default:
      return icons.UNKNOWN;
  }
}

function getIconKind(eventType) {
  switch (eventType) {
    case 'downloadFailed':
      return kinds.DANGER;
    default:
      return kinds.DEFAULT;
  }
}

function getTooltip(eventType, data) {
  switch (eventType) {
    case 'grabbed':
      return `Album grabbed from ${data.indexer} and sent to ${data.downloadClient}`;
    case 'seriesFolderImported':
      return 'Album imported from artist folder';
    case 'downloadFolderImported':
      return 'Album downloaded successfully and picked up from download client';
    case 'downloadFailed':
      return 'Album download failed';
    case 'trackFileDeleted':
      return 'Track file deleted';
    case 'trackFileRenamed':
      return 'Track file renamed';
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
