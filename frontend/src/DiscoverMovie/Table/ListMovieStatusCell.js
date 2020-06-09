import PropTypes from 'prop-types';
import React from 'react';
import { icons } from 'Helpers/Props';
import Icon from 'Components/Icon';
import VirtualTableRowCell from 'Components/Table/Cells/TableRowCell';
import { getMovieStatusDetails } from 'Movie/MovieStatus';
import styles from './ListMovieStatusCell.css';

function ListMovieStatusCell(props) {
  const {
    className,
    status,
    isExclusion,
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

      {
        isExclusion ?
          <Icon
            className={styles.exclusionIcon}
            name={icons.DANGER}
            title={'Movie Excluded From Automatic Add'}
          /> : null
      }

    </Component>
  );
}

ListMovieStatusCell.propTypes = {
  className: PropTypes.string.isRequired,
  status: PropTypes.string.isRequired,
  isExclusion: PropTypes.bool.isRequired,
  component: PropTypes.elementType
};

ListMovieStatusCell.defaultProps = {
  className: styles.status,
  component: VirtualTableRowCell
};

export default ListMovieStatusCell;
