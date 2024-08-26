import React from 'react';
import Label from 'Components/Label';
import { kinds } from 'Helpers/Props';
import { QualityModel } from 'Quality/Quality';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';

function getTooltip(
  title: string,
  quality: QualityModel,
  size: number | undefined,
  isCutoffNotMet: boolean
) {
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

  if (isCutoffNotMet) {
    title += ` [${translate('CutoffNotMet')}]`;
  }

  return title;
}

function revisionLabel(
  className: string | undefined,
  quality: QualityModel,
  showRevision: boolean
) {
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

  return null;
}

interface MovieQualityProps {
  className?: string;
  title?: string;
  quality: QualityModel;
  size?: number;
  isCutoffNotMet?: boolean;
  showRevision?: boolean;
}

function MovieQuality(props: MovieQualityProps) {
  const {
    className,
    title = '',
    quality,
    size,
    isCutoffNotMet = false,
    showRevision = false,
  } = props;

  if (!quality) {
    return null;
  }

  return (
    <span>
      <Label
        className={className}
        kind={isCutoffNotMet ? kinds.INVERSE : kinds.DEFAULT}
        title={getTooltip(title, quality, size, isCutoffNotMet)}
      >
        {quality.quality.name}
      </Label>
      {revisionLabel(className, quality, showRevision)}
    </span>
  );
}

export default MovieQuality;
