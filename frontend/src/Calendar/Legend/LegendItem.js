import classNames from 'classnames';
import PropTypes from 'prop-types';
import React from 'react';
import styles from './LegendItem.css';

function LegendItem(props) {
  const {
    name,
    style,
    colorImpairedMode
  } = props;

  return (
    <div className={styles.legendItemContainer}>
      <div
        className={classNames(
          styles.legendItem,
          styles[style],
          colorImpairedMode && 'colorImpaired'
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
  colorImpairedMode: PropTypes.bool.isRequired
};

export default LegendItem;
