import moment from 'moment';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import classNames from 'classnames';
import { icons } from 'Helpers/Props';
import getStatusStyle from 'Calendar/getStatusStyle';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
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
      foreignAlbumId,
      releaseDate,
      monitored,
      statistics,
      grabbed,
      queueItem,
      // timeFormat,
      colorImpairedMode
    } = this.props;

    if (!artist) {
      return null;
    }

    const startTime = moment(releaseDate);
    // const endTime = startTime.add(artist.runtime, 'minutes');
    const downloading = !!(queueItem || grabbed);
    const isMonitored = artist.monitored && monitored;
    const statusStyle = getStatusStyle(id, downloading, startTime, isMonitored, statistics.percentOfTracks);

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
              <Link to={`/artist/${artist.foreignArtistId}`}>
                {artist.artistName}
              </Link>
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
              <Link to={`/album/${foreignAlbumId}`}>
                {title}
              </Link>
            </div>
          </div>
        </Link>
      </div>
    );
  }
}

CalendarEvent.propTypes = {
  id: PropTypes.number.isRequired,
  artist: PropTypes.object.isRequired,
  title: PropTypes.string.isRequired,
  foreignAlbumId: PropTypes.string.isRequired,
  statistics: PropTypes.object.isRequired,
  releaseDate: PropTypes.string.isRequired,
  monitored: PropTypes.bool.isRequired,
  grabbed: PropTypes.bool,
  queueItem: PropTypes.object,
  // timeFormat: PropTypes.string.isRequired,
  colorImpairedMode: PropTypes.bool.isRequired,
  onEventModalOpenToggle: PropTypes.func.isRequired
};

CalendarEvent.defaultProps = {
  statistics: {
    percentOfTracks: 0
  }
};

export default CalendarEvent;
