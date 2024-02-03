import PropTypes from 'prop-types';
import React from 'react';
import Label from 'Components/Label';
import { kinds } from 'Helpers/Props';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';

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

function revisionLabel(className, quality, showRevision) {
  if (!showRevision) {
    return;
  }

  if (quality.revision.isRepack) {
    return (
      <Label
        className={className}
        kind={kinds.PRIMARY}
        title={translate('Repack')}
      >
        R
      </Label>
    );
  }

  if (quality.revision.version && quality.revision.version > 1) {
    return (
      <Label
        className={className}
        kind={kinds.PRIMARY}
        title={translate('Proper')}
      >
        P
      </Label>
    );
  }
}

function MovieQuality(props) {
  const {
    className,
    title,
    quality,
    size,
    isMonitored,
    isCutoffNotMet,
    showRevision
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
    <span>
      <Label
        className={className}
        kind={kind}
        title={getTooltip(title, quality, size, isMonitored, isCutoffNotMet)}
      >
        {quality.quality.name}
      </Label>{revisionLabel(className, quality, showRevision)}
    </span>
  );
}

MovieQuality.propTypes = {
  className: PropTypes.string,
  title: PropTypes.string,
  quality: PropTypes.object.isRequired,
  size: PropTypes.number,
  isMonitored: PropTypes.bool,
  isCutoffNotMet: PropTypes.bool,
  showRevision: PropTypes.bool
};

MovieQuality.defaultProps = {
  title: '',
  isMonitored: true,
  showRevision: false
};

export default MovieQuality;
