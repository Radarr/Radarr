import PropTypes from 'prop-types';
import React from 'react';
import { icons, kinds } from 'Helpers/Props';
import classNames from 'classnames';
import styles from './Icon.css';

function Icon(props) {
  const {
    className,
    name,
    kind,
    size,
    title,
    isSpinning
  } = props;

  return (
    <i
      className={classNames(
        name,
        className,
        styles[kind],
        isSpinning && icons.SPIN
      )}
      title={title}
      style={{
        fontSize: `${size}px`
      }}
    />
  );
}

Icon.propTypes = {
  className: PropTypes.string,
  name: PropTypes.string.isRequired,
  kind: PropTypes.string.isRequired,
  size: PropTypes.number.isRequired,
  title: PropTypes.string,
  isSpinning: PropTypes.bool.isRequired
};

Icon.defaultProps = {
  kind: kinds.DEFAULT,
  size: 14,
  isSpinning: false
};

export default Icon;
