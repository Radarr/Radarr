import PropTypes from 'prop-types';
import React from 'react';
import { icons } from 'Helpers/Props';
import Icon from 'Components/Icon';
import VirtualTableRowCell from 'Components/Table/Cells/TableRowCell';
import styles from './MovieStatusCell.css';

function MovieStatusCell(props) {
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
      <Icon
        className={styles.statusIcon}
        name={monitored ? icons.MONITORED : icons.UNMONITORED}
        title={monitored ? 'Movie is monitored' : 'Movie is unmonitored'}
      />

      <Icon
        className={styles.statusIcon}
        name={status === 'released' ? icons.SERIES_ENDED : icons.SERIES_CONTINUING}
        title={status === 'ended' ? 'Ended' : 'Continuing'}

      />
    </Component>
  );
}

MovieStatusCell.propTypes = {
  className: PropTypes.string.isRequired,
  monitored: PropTypes.bool.isRequired,
  status: PropTypes.string.isRequired,
  component: PropTypes.func
};

MovieStatusCell.defaultProps = {
  className: styles.status,
  component: VirtualTableRowCell
};

export default MovieStatusCell;
