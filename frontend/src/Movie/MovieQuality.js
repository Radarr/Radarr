import PropTypes from 'prop-types';
import React from 'react';
import Label from 'Components/Label';
import { kinds } from 'Helpers/Props';
import formatBytes from 'Utilities/Number/formatBytes';

function getTooltip(title, quality, size, isMonitored, isCutoffNotMet) {
  const revision = quality.revision;

  if (revision.real && revision.real > 0) {
    title += ' [REAL]';
  }

  if (revision.version && revision.version > 1) {
    title += ' [PROPER]';
  }

  if (size) {
    title += ` - ${formatBytes(size)}`;
  }

  if (!isMonitored) {
    title += ' [Not Monitored]';
  } else if (isCutoffNotMet) {
    title += ' [Cutoff Not Met]';
  }

  return title;
}

function MovieQuality(props) {
  const {
    className,
    title,
    quality,
    size,
    isMonitored,
    isCutoffNotMet
  } = props;

  let kind = kinds.DEFAULT;
  if (!isMonitored) {
    kind = kinds.DISABLED;
  } else if (isCutoffNotMet) {
    kind = kinds.INVERSE;
  }

  if (!quality) {
    return null;
  }

  return (
    <Label
      className={className}
      kind={kind}
      title={getTooltip(title, quality, size, isMonitored, isCutoffNotMet)}
    >
      {quality.quality.name}
    </Label>
  );
}

MovieQuality.propTypes = {
  className: PropTypes.string,
  title: PropTypes.string,
  quality: PropTypes.object.isRequired,
  size: PropTypes.number,
  isMonitored: PropTypes.bool,
  isCutoffNotMet: PropTypes.bool
};

MovieQuality.defaultProps = {
  title: '',
  isMonitored: true
};

export default MovieQuality;
