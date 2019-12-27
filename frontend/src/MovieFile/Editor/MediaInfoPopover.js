import PropTypes from 'prop-types';
import React, { Component } from 'react';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import styles from './MediaInfoPopover.css';

function mapObjectproperties(object) {
  return Object.keys(object).map((key) => {
    const title = key
      .replace(/([A-Z])/g, ' $1')
      .replace(/^./, (str) => str.toUpperCase());

    const value = object[key];

    if (!value || key === 'videoStreams' || key === 'audioStreams') {
      return null;
    }

    return (
      <DescriptionListItem
        key={key}
        title={title}
        data={value}
      />
    );
  });
}

class MediaInfoPopover extends Component {
  render() {
    const {
      mediaInfo
    } = this.props;

    return (
      <div>
        <DescriptionList>
          {
            mapObjectproperties(mediaInfo)
          }
        </DescriptionList>
        {
          mediaInfo.audioStreams.map((audioStream, index) => {
            return (
              <DescriptionList key={index} className={styles.stream}>
                {
                  mapObjectproperties(audioStream)
                }
              </DescriptionList>
            );
          })
        }
        {
          mediaInfo.videoStreams.map((videoStream, index) => {
            return (
              <DescriptionList key={index} className={styles.stream}>
                {
                  mapObjectproperties(videoStream)
                }
              </DescriptionList>
            );
          })
        }
      </div>
    );
  }
}

MediaInfoPopover.propTypes = {
  mediaInfo: PropTypes.object.isRequired
};

export default MediaInfoPopover;
