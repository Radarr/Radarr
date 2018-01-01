import moment from 'moment';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import classNames from 'classnames';
import { icons } from 'Helpers/Props';
import getStatusStyle from 'Calendar/getStatusStyle';
import albumEntities from 'Album/albumEntities';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import AlbumDetailsModal from 'Album/AlbumDetailsModal';
import CalendarEventQueueDetails from './CalendarEventQueueDetails';
import styles from './CalendarEvent.css';

class CalendarEvent extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isDetailsModalOpen: false
    };
  }

  //
  // Listeners

  onPress = () => {
    this.setState({ isDetailsModalOpen: true }, () => {
      this.props.onEventModalOpenToggle(true);
    });
  }

  onDetailsModalClose = () => {
    this.setState({ isDetailsModalOpen: false }, () => {
      this.props.onEventModalOpenToggle(false);
    });
  }

  //
  // Render

  render() {
    const {
      id,
      artist,
      title,
      // seasonNumber,
      // episodeNumber,
      // absoluteEpisodeNumber,
      releaseDate,
      monitored,
      // hasFile,
      grabbed,
      queueItem,
      // timeFormat,
      colorImpairedMode
    } = this.props;

    const startTime = moment(releaseDate);
    // const endTime = startTime.add(artist.runtime, 'minutes');
    const downloading = !!(queueItem || grabbed);
    const isMonitored = artist.monitored && monitored;
    const statusStyle = getStatusStyle(id, downloading, startTime, isMonitored);
    // const missingAbsoluteNumber = artist.artistType === 'anime' && seasonNumber > 0 && !absoluteEpisodeNumber;

    return (
      <div>
        <Link
          className={classNames(
            styles.event,
            styles[statusStyle],
            colorImpairedMode && 'colorImpaired'
          )}
          component="div"
          onPress={this.onPress}
        >
          <div className={styles.info}>
            <div className={styles.artistName}>
              {artist.artistName}
            </div>

            {
              !!queueItem &&
                <span className={styles.statusIcon}>
                  <CalendarEventQueueDetails
                    {...queueItem}
                  />
                </span>
            }

            {
              !queueItem && grabbed &&
                <Icon
                  className={styles.statusIcon}
                  name={icons.DOWNLOADING}
                  title="Album is downloading"
                />
            }
          </div>

          <div className={styles.albumInfo}>
            <div className={styles.albumTitle}>
              {title}
            </div>
          </div>
        </Link>

        <AlbumDetailsModal
          isOpen={this.state.isDetailsModalOpen}
          albumId={id}
          albumEntity={albumEntities.CALENDAR}
          artistId={artist.id}
          albumTitle={title}
          showOpenArtistButton={true}
          onModalClose={this.onDetailsModalClose}
        />
      </div>
    );
  }
}

CalendarEvent.propTypes = {
  id: PropTypes.number.isRequired,
  artist: PropTypes.object.isRequired,
  title: PropTypes.string.isRequired,
  // seasonNumber: PropTypes.number.isRequired,
  // episodeNumber: PropTypes.number.isRequired,
  // absoluteEpisodeNumber: PropTypes.number,
  releaseDate: PropTypes.string.isRequired,
  monitored: PropTypes.bool.isRequired,
  // hasFile: PropTypes.bool.isRequired,
  grabbed: PropTypes.bool,
  queueItem: PropTypes.object,
  // timeFormat: PropTypes.string.isRequired,
  colorImpairedMode: PropTypes.bool.isRequired,
  onEventModalOpenToggle: PropTypes.func.isRequired
};

export default CalendarEvent;
