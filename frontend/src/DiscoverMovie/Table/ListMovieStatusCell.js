import PropTypes from 'prop-types';
import React from 'react';
import Icon from 'Components/Icon';
import VirtualTableRowCell from 'Components/Table/Cells/TableRowCell';
import getMovieStatusDetails from 'Movie/getMovieStatusDetails';
import styles from './ListMovieStatusCell.css';

function ListMovieStatusCell(props) {
  const {
    className,
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
        name={statusDetails.icon}
        title={`${statusDetails.title}: ${statusDetails.message}`}
      />

    </Component>
  );
}

ListMovieStatusCell.propTypes = {
  className: PropTypes.string.isRequired,
  status: PropTypes.string.isRequired,
  component: PropTypes.elementType
};

ListMovieStatusCell.defaultProps = {
  className: styles.status,
  component: VirtualTableRowCell
};

export default ListMovieStatusCell;
