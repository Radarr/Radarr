import moment from 'moment';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import classNames from 'classnames';
import { icons, kinds } from 'Helpers/Props';
import getStatusStyle from 'Calendar/getStatusStyle';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import CalendarEventQueueDetails from './CalendarEventQueueDetails';
import styles from './CalendarEvent.css';

class CalendarEvent extends Component {

  //
  // Render

  render() {
    const {
      movieFile,
      inCinemas,
      title,
      titleSlug,
      genres,
      monitored,
      hasFile,
      grabbed,
      queueItem,
      showMovieInformation,
      showCutoffUnmetIcon,
      colorImpairedMode,
      date
    } = this.props;

    const startTime = moment(inCinemas);
    const isDownloading = !!(queueItem || grabbed);
    const isMonitored = monitored;
    const statusStyle = getStatusStyle(hasFile, isDownloading, startTime, isMonitored);
    const joinedGenres = genres.slice(0, 2).join(', ');
    const link = `/movie/${titleSlug}`;
    const eventType = moment(date).isSame(moment(inCinemas), 'day') ? 'In Cinemas' : 'Physical Release';

    return (
      <div>
        <Link
          className={classNames(
            styles.event,
            styles.link,
            styles[statusStyle],
            colorImpairedMode && 'colorImpaired'
          )}
          // component="div"
          to={link}
        >
          <div className={styles.info}>
            <div className={styles.movieTitle}>
              {title}
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

          {
            showMovieInformation &&
              <div className={styles.movieInfo}>
                <div className={styles.genres}>
                  {joinedGenres}
                </div>
              </div>
          }

          {
            showMovieInformation &&
              <div className={styles.movieInfo}>
                <div className={styles.genres}>
                  {eventType}
                </div>
              </div>
          }
        </Link>

      </div>
    );
  }
}

CalendarEvent.propTypes = {
  id: PropTypes.number.isRequired,
  genres: PropTypes.arrayOf(PropTypes.string).isRequired,
  movieFile: PropTypes.object,
  title: PropTypes.string.isRequired,
  titleSlug: PropTypes.string.isRequired,
  inCinemas: PropTypes.string.isRequired,
  monitored: PropTypes.bool.isRequired,
  hasFile: PropTypes.bool.isRequired,
  grabbed: PropTypes.bool,
  queueItem: PropTypes.object,
  showMovieInformation: PropTypes.bool.isRequired,
  showCutoffUnmetIcon: PropTypes.bool.isRequired,
  timeFormat: PropTypes.string.isRequired,
  colorImpairedMode: PropTypes.bool.isRequired,
  date: PropTypes.string.isRequired
};

CalendarEvent.defaultProps = {
  genres: []
};

export default CalendarEvent;
