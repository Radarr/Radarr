import classNames from 'classnames';
import React from 'react';
import { PosterReplacementColors } from './usePosterReplacement';
import styles from './ReplacementPoster.css';

// Font size as a percentage of the poster width, shrinking for longer titles.
// Approximates the server rendered poster which shrinks the font until the title fits.
function getFontSize(title: string) {
  const length = title.length;

  if (length <= 10) {
    return 14;
  }

  if (length <= 20) {
    return 12;
  }

  if (length <= 40) {
    return 10;
  }

  if (length <= 70) {
    return 8;
  }

  return 6.5;
}

interface ReplacementPosterProps extends PosterReplacementColors {
  className?: string;
  style?: object;
  title: string;
}

function ReplacementPoster({
  className,
  style,
  title,
  backgroundColor,
  textColor,
}: ReplacementPosterProps) {
  return (
    <div
      className={classNames(className, styles.poster)}
      style={{ ...style, backgroundColor, color: textColor }}
    >
      <div className={styles.content}>
        <div
          className={styles.title}
          style={{ fontSize: `${getFontSize(title)}cqi` }}
        >
          {title}
        </div>
      </div>
    </div>
  );
}

export default ReplacementPoster;
