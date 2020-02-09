import PropTypes from 'prop-types';
import React from 'react';
import formatBytes from 'Utilities/Number/formatBytes';
import EnhancedSelectInputSelectedValue from './EnhancedSelectInputSelectedValue';
import styles from './RootFolderSelectInputSelectedValue.css';

function RootFolderSelectInputSelectedValue(props) {
  const {
    name,
    value,
    freeSpace,
    includeFreeSpace,
    ...otherProps
  } = props;

  const text = value === '' ? name : `${name} [${value}]`;

  return (
    <EnhancedSelectInputSelectedValue
      className={styles.selectedValue}
      {...otherProps}
    >
      <div className={styles.path}>
        {text}
      </div>

      {
        freeSpace != null && includeFreeSpace &&
          <div className={styles.freeSpace}>
            {formatBytes(freeSpace)} Free
          </div>
      }
    </EnhancedSelectInputSelectedValue>
  );
}

RootFolderSelectInputSelectedValue.propTypes = {
  name: PropTypes.string,
  value: PropTypes.string,
  freeSpace: PropTypes.number,
  includeFreeSpace: PropTypes.bool.isRequired
};

RootFolderSelectInputSelectedValue.defaultProps = {
  includeFreeSpace: true
};

export default RootFolderSelectInputSelectedValue;
