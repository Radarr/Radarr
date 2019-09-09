import moment from 'moment';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import classNames from 'classnames';
import formatTime from 'Utilities/Date/formatTime';
import { icons } from 'Helpers/Props';
import getStatusStyle from 'Calendar/getStatusStyle';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
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
      foreignAlbumId,
      releaseDate,
      monitored,
      statistics,
      grabbed,
      queueItem,
      showDate,
      timeFormat,
      longDateFormat,
      colorImpairedMode
    } = this.props;

    const startTime = moment(releaseDate);
    // const endTime = startTime.add(artist.runtime, 'minutes');
    const downloading = !!(queueItem || grabbed);
    const isMonitored = artist.monitored && monitored;
    const statusStyle = getStatusStyle(id, downloading, startTime, isMonitored, statistics.percentOfTracks);

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
              styles.eventWrapper,
              styles[statusStyle],
              colorImpairedMode && 'colorImpaired'
            )}
          />

          <div className={styles.time}>
            {formatTime(releaseDate, timeFormat)}
          </div>

          <div className={styles.artistName}>
            <Link to={`/artist/${artist.foreignArtistId}`}>
              {artist.artistName}
            </Link>
          </div>

          <div className={styles.albumSeparator}> - </div>

          <div className={styles.albumTitle}>
            <Link to={`/album/${foreignAlbumId}`}>
              {title}
            </Link>
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
      </div>
    );
  }
}

AgendaEvent.propTypes = {
  id: PropTypes.number.isRequired,
  artist: PropTypes.object.isRequired,
  title: PropTypes.string.isRequired,
  foreignAlbumId: PropTypes.string.isRequired,
  albumType: PropTypes.string.isRequired,
  releaseDate: PropTypes.string.isRequired,
  monitored: PropTypes.bool.isRequired,
  statistics: PropTypes.object.isRequired,
  grabbed: PropTypes.bool,
  queueItem: PropTypes.object,
  showDate: PropTypes.bool.isRequired,
  timeFormat: PropTypes.string.isRequired,
  colorImpairedMode: PropTypes.bool.isRequired,
  longDateFormat: PropTypes.string.isRequired
};

AgendaEvent.defaultProps = {
  statistics: {
    percentOfTracks: 0
  }
};

export default AgendaEvent;
