import PropTypes from 'prop-types';
import React from 'react';
import * as mediaInfoTypes from './mediaInfoTypes';

function MediaInfo(props) {
  const {
    type,
    audioChannels,
    audioCodec,
    audioBitRate,
    audioBits,
    audioSampleRate
  } = props;

  if (type === mediaInfoTypes.AUDIO) {
    return (
      <span>
        {
          !!audioCodec &&
            audioCodec
        }

        {
          !!audioCodec && !!audioChannels &&
          ' - '
        }

        {
          !!audioChannels &&
          audioChannels.toFixed(1)
        }

        {
          ((!!audioCodec && !!audioBitRate) || (!!audioChannels && !!audioBitRate)) &&
          ' - '
        }

        {
          !!audioBitRate &&
            audioBitRate
        }

        {
          ((!!audioCodec && !!audioSampleRate) || (!!audioChannels && !!audioSampleRate) || (!!audioBitRate && !!audioSampleRate)) &&
          ' - '
        }

        {
          !!audioSampleRate &&
          audioSampleRate
        }

        {
          ((!!audioCodec && !!audioBits) || (!!audioChannels && !!audioBits) || (!!audioBitRate && !!audioBits) || (!!audioSampleRate && !!audioBits)) &&
          ' - '
        }

        {
          !!audioBits &&
          audioBits
        }
      </span>
    );
  }

  return null;
}

MediaInfo.propTypes = {
  type: PropTypes.string.isRequired,
  audioChannels: PropTypes.number,
  audioCodec: PropTypes.string,
  audioBitRate: PropTypes.string,
  audioBits: PropTypes.string,
  audioSampleRate: PropTypes.string
};

export default MediaInfo;
