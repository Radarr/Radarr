import PropTypes from 'prop-types';
import React from 'react';
import { icons } from 'Helpers/Props';
import Icon from 'Components/Icon';
import VirtualTableRowCell from 'Components/Table/Cells/TableRowCell';
import styles from './MovieStatusCell.css';

function MovieStatusCell(props) {
  const {
    className,
    status,
    component: Component,
    ...otherProps
  } = props;

  return (
    <Component
      className={className}
      {...otherProps}
    >
      {
        status === 'announced' ?
          <Icon
            className={styles.statusIcon}
            name={icons.ANNOUNCED}
            title={'Movie is announced'}
          /> : null
      }

      {
        status === 'inCinemas' ?
          <Icon
            className={styles.statusIcon}
            name={icons.IN_CINEMAS}
            title={'Movie is in Cinemas'}
          /> : null
      }

      {
        status === 'released' ?
          <Icon
            className={styles.statusIcon}
            name={icons.MOVIE_FILE}
            title={'Movie is released'}
          /> : null
      }
    </Component>
  );
}

MovieStatusCell.propTypes = {
  className: PropTypes.string.isRequired,
  status: PropTypes.string.isRequired,
  component: PropTypes.elementType
};

MovieStatusCell.defaultProps = {
  className: styles.status,
  component: VirtualTableRowCell
};

export default MovieStatusCell;
