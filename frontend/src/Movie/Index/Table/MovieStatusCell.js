import PropTypes from 'prop-types';
import React from 'react';
import Icon from 'Components/Icon';
import VirtualTableRowCell from 'Components/Table/Cells/TableRowCell';
import { icons } from 'Helpers/Props';
import { getMovieStatusDetails } from 'Movie/MovieStatus';
import styles from './MovieStatusCell.css';

function MovieStatusCell(props) {
  const {
    className,
    monitored,
    status,
    component: Component,
    ...otherProps
  } = props;

  const statusDetails = getMovieStatusDetails(status);

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
        name={statusDetails.icon}
        title={`${statusDetails.title}: ${statusDetails.message}`}
      />

    </Component>
  );
}

MovieStatusCell.propTypes = {
  className: PropTypes.string.isRequired,
  monitored: PropTypes.bool.isRequired,
  status: PropTypes.string.isRequired,
  component: PropTypes.elementType
};

MovieStatusCell.defaultProps = {
  className: styles.status,
  component: VirtualTableRowCell
};

export default MovieStatusCell;
