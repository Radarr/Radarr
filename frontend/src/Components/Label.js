import classNames from 'classnames';
import PropTypes from 'prop-types';
import React from 'react';
import { kinds, sizes } from 'Helpers/Props';
import styles from './Label.css';

function Label(props) {
  const {
    className,
    kind,
    size,
    outline,
    children,
    colorImpairedMode,
    ...otherProps
  } = props;

  return (
    <span
      className={classNames(
        className,
        styles[kind],
        styles[size],
        outline && styles.outline,
        colorImpairedMode && 'colorImpaired'
      )}
      {...otherProps}
    >
      {children}
    </span>
  );
}

Label.propTypes = {
  className: PropTypes.string.isRequired,
  kind: PropTypes.oneOf(kinds.all).isRequired,
  size: PropTypes.oneOf(sizes.all).isRequired,
  outline: PropTypes.bool.isRequired,
  children: PropTypes.node.isRequired,
  colorImpairedMode: PropTypes.bool
};

Label.defaultProps = {
  className: styles.label,
  kind: kinds.DEFAULT,
  size: sizes.SMALL,
  outline: false,
  colorImpairedMode: false
};

export default Label;
