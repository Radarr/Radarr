import classNames from 'classnames';
import PropTypes from 'prop-types';
import React from 'react';
import { kinds, sizes } from 'Helpers/Props';
import styles from './InfoLabel.css';

function InfoLabel(props) {
  const {
    className,
    name,
    kind,
    size,
    outline,
    children,
    ...otherProps
  } = props;

  return (
    <span
      className={classNames(
        className,
        styles[kind],
        styles[size],
        outline && styles.outline
      )}
      {...otherProps}
    >
      <div className={styles.name}>
        {name}
      </div>
      <div>
        {children}
      </div>
    </span>
  );
}

InfoLabel.propTypes = {
  className: PropTypes.string.isRequired,
  name: PropTypes.string.isRequired,
  kind: PropTypes.oneOf(kinds.all).isRequired,
  size: PropTypes.oneOf(sizes.all).isRequired,
  outline: PropTypes.bool.isRequired,
  children: PropTypes.node.isRequired
};

InfoLabel.defaultProps = {
  className: styles.label,
  kind: kinds.DEFAULT,
  size: sizes.SMALL,
  outline: false
};

export default InfoLabel;
