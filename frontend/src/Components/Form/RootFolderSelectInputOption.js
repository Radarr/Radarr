import classNames from 'classnames';
import PropTypes from 'prop-types';
import React from 'react';
import formatBytes from 'Utilities/Number/formatBytes';
import EnhancedSelectInputOption from './EnhancedSelectInputOption';
import styles from './RootFolderSelectInputOption.css';

function RootFolderSelectInputOption(props) {
  const {
    value,
    name,
    freeSpace,
    isMobile,
    ...otherProps
  } = props;

  const text = value === '' ? name : `${name} [${value}]`;

  return (
    <EnhancedSelectInputOption
      isMobile={isMobile}
      {...otherProps}
    >
      <div className={classNames(
        styles.optionText,
        isMobile && styles.isMobile
      )}
      >
        <div>{text}</div>

        {
          freeSpace != null &&
            <div className={styles.freeSpace}>
              {formatBytes(freeSpace)} Free
            </div>
        }
      </div>
    </EnhancedSelectInputOption>
  );
}

RootFolderSelectInputOption.propTypes = {
  name: PropTypes.string.isRequired,
  value: PropTypes.string.isRequired,
  freeSpace: PropTypes.number,
  isMobile: PropTypes.bool.isRequired
};

export default RootFolderSelectInputOption;
