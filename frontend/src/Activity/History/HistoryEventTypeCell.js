import PropTypes from 'prop-types';
import React from 'react';
import Icon from 'Components/Icon';
import TableRowCell from 'Components/Table/Cells/TableRowCell';
import { icons, kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './HistoryEventTypeCell.css';

function getIconName(eventType) {
  switch (eventType) {
    case 'grabbed':
      return icons.DOWNLOADING;
    case 'movieFolderImported':
      return icons.DRIVE;
    case 'downloadFolderImported':
      return icons.DOWNLOADED;
    case 'downloadFailed':
      return icons.DOWNLOADING;
    case 'movieFileDeleted':
      return icons.DELETE;
    case 'movieFileRenamed':
      return icons.ORGANIZE;
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
    default:
      return kinds.DEFAULT;
  }
}

function getTooltip(eventType, data) {
  switch (eventType) {
    case 'grabbed':
      return translate('MovieGrabbedHistoryTooltip', { indexer: data.indexer, downloadClient: data.downloadClient });
    case 'movieFolderImported':
      return translate('MovieFolderImportedTooltip');
    case 'downloadFolderImported':
      return translate('MovieImportedTooltip');
    case 'downloadFailed':
      return translate('MovieDownloadFailedTooltip');
    case 'movieFileDeleted':
      return translate('MovieFileDeletedTooltip');
    case 'movieFileRenamed':
      return translate('MovieFileRenamedTooltip');
    case 'downloadIgnored':
      return translate('MovieDownloadIgnoredTooltip');
    default:
      return translate('UnknownEventTooltip');
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
