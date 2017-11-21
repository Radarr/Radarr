import PropTypes from 'prop-types';
import React from 'react';
import * as mediaInfoTypes from './mediaInfoTypes';

function MediaInfo(props) {
  const {
    type,
    audioChannels,
    audioCodec,
    audioBitRate,
    videoCodec
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
      </span>
    );
  }

  if (type === mediaInfoTypes.VIDEO) {
    return (
      <span>
        {videoCodec}
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
  videoCodec: PropTypes.string
};

export default MediaInfo;
