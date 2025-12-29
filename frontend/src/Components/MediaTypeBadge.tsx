import React from 'react';
import Label from 'Components/Label';
import { kinds, sizes } from 'Helpers/Props';
import { MediaType } from 'Movie/Movie';
import translate from 'Utilities/String/translate';

interface MediaTypeBadgeProps {
  mediaType?: MediaType;
  className?: string;
}

function getKindForMediaType(mediaType?: MediaType) {
  switch (mediaType) {
    case 'book':
      return kinds.INFO;
    case 'audiobook':
      return kinds.SUCCESS;
    case 'movie':
    default:
      return kinds.PRIMARY;
  }
}

function getLabelForMediaType(mediaType?: MediaType) {
  switch (mediaType) {
    case 'book':
      return translate('Book');
    case 'audiobook':
      return translate('Audiobook');
    case 'movie':
    default:
      return translate('Movie');
  }
}

function MediaTypeBadge({
  mediaType,
  className,
}: Readonly<MediaTypeBadgeProps>) {
  return (
    <Label
      className={className}
      kind={getKindForMediaType(mediaType)}
      size={sizes.SMALL}
    >
      {getLabelForMediaType(mediaType)}
    </Label>
  );
}

export default MediaTypeBadge;
