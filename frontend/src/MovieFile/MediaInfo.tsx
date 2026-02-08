import React from 'react';
import useLanguageName from 'Language/useLanguageName';
import translate from 'Utilities/String/translate';
import useMovieFile from './useMovieFile';

function formatLanguages(
  languages: string | undefined,
  getLanguageName: (code: string) => string
) {
  if (!languages) {
    return null;
  }

  const splitLanguages = [...new Set(languages.split('/'))].map((l) => {
    const simpleLanguage = l.split('_')[0];

    if (simpleLanguage === 'und') {
      return translate('Unknown');
    }

    return getLanguageName(simpleLanguage);
  });

  if (splitLanguages.length > 3) {
    return (
      <span title={splitLanguages.join(', ')}>
        {splitLanguages.slice(0, 2).join(', ')}, {splitLanguages.length - 2}{' '}
        more
      </span>
    );
  }

  return <span>{splitLanguages.join(', ')}</span>;
}

export type MediaInfoType =
  | 'audio'
  | 'audioLanguages'
  | 'subtitles'
  | 'video'
  | 'videoDynamicRangeType';

interface MediaInfoProps {
  movieFileId?: number;
  type: MediaInfoType;
}

function MediaInfo({ movieFileId, type }: MediaInfoProps) {
  const getLanguageName = useLanguageName();
  const movieFile = useMovieFile(movieFileId);

  if (!movieFile?.mediaInfo) {
    return null;
  }

  const {
    audioChannels,
    audioCodec,
    audioLanguages,
    subtitles,
    videoCodec,
    videoDynamicRangeType,
  } = movieFile.mediaInfo;

  if (type === 'audio') {
    return (
      <span>
        {audioCodec ? audioCodec : ''}

        {audioCodec && audioChannels ? ' - ' : ''}

        {audioChannels ? audioChannels.toFixed(1) : ''}
      </span>
    );
  }

  if (type === 'audioLanguages') {
    return formatLanguages(audioLanguages, getLanguageName);
  }

  if (type === 'subtitles') {
    return formatLanguages(subtitles, getLanguageName);
  }

  if (type === 'video') {
    return <span>{videoCodec}</span>;
  }

  if (type === 'videoDynamicRangeType') {
    return <span>{videoDynamicRangeType}</span>;
  }

  return null;
}

export default MediaInfo;
