import PropTypes from 'prop-types';
import React from 'react';
import styles from './VirtualTableRowCell.css';

function VirtualTableRowCell(props) {
  const {
    className,
    children,
    title
  } = props;

  return (
    <div
      className={className}
      title={title}
    >
      {children}
    </div>
  );
}

VirtualTableRowCell.propTypes = {
  className: PropTypes.string.isRequired,
  children: PropTypes.oneOfType([PropTypes.string, PropTypes.node]),
  title: PropTypes.string
};

VirtualTableRowCell.defaultProps = {
  className: styles.cell,
  title: ''
};

export default VirtualTableRowCell;
