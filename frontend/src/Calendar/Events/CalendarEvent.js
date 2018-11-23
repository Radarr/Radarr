import moment from 'moment';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import classNames from 'classnames';
import { icons, kinds } from 'Helpers/Props';
import getStatusStyle from 'Calendar/getStatusStyle';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import CalendarEventQueueDetails from './CalendarEventQueueDetails';
import MovieTitleLink from 'Movie/MovieTitleLink';
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
      movieFile,
      inCinemas,
      title,
      titleSlug,
      monitored,
      hasFile,
      grabbed,
      queueItem,
      showCutoffUnmetIcon,
      colorImpairedMode
    } = this.props;

    const startTime = moment(inCinemas);
    const isDownloading = !!(queueItem || grabbed);
    const isMonitored = monitored;
    const statusStyle = getStatusStyle(hasFile, isDownloading, startTime, isMonitored);

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
            <div className={styles.seriesTitle}>
              <MovieTitleLink
                titleSlug={titleSlug}
                title={title}
              />
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
                  title="movie is downloading"
                />
            }

            {
              showCutoffUnmetIcon &&
              !!movieFile &&
              movieFile.qualityCutoffNotMet &&
                <Icon
                  className={styles.statusIcon}
                  name={icons.MOVIE_FILE}
                  kind={kinds.WARNING}
                  title="Quality cutoff has not been met"
                />
            }
          </div>
        </Link>

      </div>
    );
  }
}

CalendarEvent.propTypes = {
  id: PropTypes.number.isRequired,
  movieFile: PropTypes.object,
  title: PropTypes.string.isRequired,
  titleSlug: PropTypes.string.isRequired,
  inCinemas: PropTypes.string.isRequired,
  monitored: PropTypes.bool.isRequired,
  hasFile: PropTypes.bool.isRequired,
  grabbed: PropTypes.bool,
  queueItem: PropTypes.object,
  showCutoffUnmetIcon: PropTypes.bool.isRequired,
  timeFormat: PropTypes.string.isRequired,
  colorImpairedMode: PropTypes.bool.isRequired,
  onEventModalOpenToggle: PropTypes.func.isRequired
};

export default CalendarEvent;
