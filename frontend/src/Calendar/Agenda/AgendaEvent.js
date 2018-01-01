import moment from 'moment';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import classNames from 'classnames';
import formatTime from 'Utilities/Date/formatTime';
import { icons } from 'Helpers/Props';
import getStatusStyle from 'Calendar/getStatusStyle';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import albumEntities from 'Album/albumEntities';
import AlbumDetailsModal from 'Album/AlbumDetailsModal';
import CalendarEventQueueDetails from 'Calendar/Events/CalendarEventQueueDetails';
import styles from './AgendaEvent.css';

class AgendaEvent extends Component {
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
    this.setState({ isDetailsModalOpen: true });
  }

  onDetailsModalClose = () => {
    this.setState({ isDetailsModalOpen: false });
  }

  //
  // Render

  render() {
    const {
      id,
      artist,
      title,
      releaseDate,
      monitored,
      hasFile,
      grabbed,
      queueItem,
      showDate,
      timeFormat,
      longDateFormat
    } = this.props;

    const startTime = moment(releaseDate);
    // const endTime = startTime.add(artist.runtime, 'minutes');
    const downloading = !!(queueItem || grabbed);
    const isMonitored = artist.monitored && monitored;
    const statusStyle = getStatusStyle(id, hasFile, downloading, startTime, isMonitored);

    return (
      <div>
        <Link
          className={styles.event}
          component="div"
          onPress={this.onPress}
        >
          <div className={styles.date}>
            {
              showDate &&
                startTime.format(longDateFormat)
            }
          </div>

          <div
            className={classNames(
              styles.status,
              styles[statusStyle]
            )}
          />

          <div className={styles.time}>
            {formatTime(releaseDate, timeFormat)}
          </div>

          <div className={styles.artistName}>
            {artist.artistName}
          </div>

          <div className={styles.albumSeparator}> - </div>

          <div className={styles.albumTitle}>
            {title}
          </div>

          {
            !!queueItem &&
              <CalendarEventQueueDetails
                {...queueItem}
              />
          }

          {
            !queueItem && grabbed &&
              <Icon
                name={icons.DOWNLOADING}
                title="Album is downloading"
              />
          }
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

AgendaEvent.propTypes = {
  id: PropTypes.number.isRequired,
  artist: PropTypes.object.isRequired,
  title: PropTypes.string.isRequired,
  releaseDate: PropTypes.string.isRequired,
  monitored: PropTypes.bool.isRequired,
  hasFile: PropTypes.bool.isRequired,
  grabbed: PropTypes.bool,
  queueItem: PropTypes.object,
  showDate: PropTypes.bool.isRequired,
  timeFormat: PropTypes.string.isRequired,
  longDateFormat: PropTypes.string.isRequired
};

export default AgendaEvent;
