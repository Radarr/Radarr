import classNames from 'classnames';
import PropTypes from 'prop-types';
import React from 'react';
import styles from './LegendItem.css';

function LegendItem(props) {
  const {
    name,
    style,
    fullColorEvents,
    colorImpairedMode
  } = props;

  return (
    <div className={styles.legendItemContainer}>
      <div
        className={classNames(
          styles.legendItem,
          styles[style],
          colorImpairedMode && 'colorImpaired',
          fullColorEvents && 'fullColor'
        )}
      />
      <div className={classNames(styles.legendItemText, colorImpairedMode && styles[`${style}ColorImpaired`])}>
        {name}
      </div>
    </div>
  );
}

LegendItem.propTypes = {
  name: PropTypes.string.isRequired,
  style: PropTypes.string.isRequired,
  fullColorEvents: PropTypes.bool.isRequired,
  colorImpairedMode: PropTypes.bool.isRequired
};

export default LegendItem;
