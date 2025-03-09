import classNames from 'classnames';
import React, { ComponentProps, ReactNode } from 'react';
import { Kind } from 'Helpers/Props/kinds';
import { Size } from 'Helpers/Props/sizes';
import styles from './InfoLabel.css';

interface InfoLabelProps extends ComponentProps<'span'> {
  className?: string;
  name: string;
  kind?: Extract<Kind, keyof typeof styles>;
  size?: Extract<Size, keyof typeof styles>;
  outline?: boolean;
  children: ReactNode;
}

function InfoLabel({
  className = styles.label,
  name,
  kind = 'default',
  size = 'small',
  outline = false,
  children,
  ...otherProps
}: InfoLabelProps) {
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
      <div className={styles.name}>{name}</div>
      <div>{children}</div>
    </span>
  );
}

export default InfoLabel;
