import PropTypes from 'prop-types';
import React from 'react';
import { icons } from 'Helpers/Props';
import Icon from 'Components/Icon';
import VirtualTableRowCell from 'Components/Table/Cells/TableRowCell';
import styles from './ArtistStatusCell.css';

function ArtistStatusCell(props) {
  const {
    className,
    monitored,
    status,
    component: Component,
    ...otherProps
  } = props;

  return (
    <Component
      className={className}
      {...otherProps}
    >
      <span title={monitored ? 'Artist is monitored' : 'Artist is unmonitored'}>
        <Icon
          className={styles.statusIcon}
          name={monitored ? icons.MONITORED : icons.UNMONITORED}
        />
      </span>

      <span title={status === 'ended' ? 'Ended' : 'Continuing'}>
        <Icon
          className={styles.statusIcon}
          name={status === 'ended' ? icons.ARTIST_ENDED : icons.ARTIST_CONTINUING}
        />
      </span>
    </Component>
  );
}

ArtistStatusCell.propTypes = {
  className: PropTypes.string.isRequired,
  monitored: PropTypes.bool.isRequired,
  status: PropTypes.string.isRequired,
  component: PropTypes.func
};

ArtistStatusCell.defaultProps = {
  className: styles.status,
  component: VirtualTableRowCell
};

export default ArtistStatusCell;
