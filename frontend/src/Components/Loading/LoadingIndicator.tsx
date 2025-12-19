import classNames from 'classnames';
import React, { useMemo } from 'react';
import styles from './LoadingIndicator.css';

interface LoadingIndicatorProps {
  className?: string;
  rippleClassName?: string;
  size?: number;
}

function LoadingIndicator({
  className = styles.loading,
  rippleClassName = styles.ripple,
  size = 50,
}: Readonly<LoadingIndicatorProps>) {
  const sizeInPx = `${size}px`;

  const containerStyle = useMemo(() => ({ height: sizeInPx }), [sizeInPx]);
  const rippleContainerStyle = useMemo(
    () => ({ width: sizeInPx, height: sizeInPx }),
    [sizeInPx]
  );

  return (
    <div className={className} style={containerStyle}>
      <div
        className={classNames(styles.rippleContainer, 'followingBalls')}
        style={rippleContainerStyle}
      >
        <div className={rippleClassName} style={rippleContainerStyle} />

        <div className={rippleClassName} style={rippleContainerStyle} />

        <div className={rippleClassName} style={rippleContainerStyle} />
      </div>
    </div>
  );
}

export default LoadingIndicator;
