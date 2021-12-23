import classNames from 'classnames';
import moment from 'moment';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import { icons, kinds } from 'Helpers/Props';
import getStatusStyle from 'Utilities/Movie/getStatusStyle';
import translate from 'Utilities/String/translate';
import CalendarEventQueueDetails from './CalendarEventQueueDetails';
import styles from './CalendarEvent.css';

class CalendarEvent extends Component {

  //
  // Render

  render() {
    const {
      movieFile,
      isAvailable,
      inCinemas,
      physicalRelease,
      digitalRelease,
      title,
      titleSlug,
      genres,
      monitored,
      certification,
      hasFile,
      grabbed,
      queueItem,
      showMovieInformation,
      showCutoffUnmetIcon,
      colorImpairedMode,
      date
    } = this.props;

    const isDownloading = !!(queueItem || grabbed);
    const isMonitored = monitored;
    const statusStyle = getStatusStyle(null, isMonitored, hasFile, isAvailable, 'style', isDownloading);
    const joinedGenres = genres.slice(0, 2).join(', ');
    const link = `/movie/${titleSlug}`;
    const eventType = [];

    if (inCinemas && moment(date).isSame(moment(inCinemas), 'day')) {
      eventType.push('Cinemas');
    }

    if (physicalRelease && moment(date).isSame(moment(physicalRelease), 'day')) {
      eventType.push('Physical');
    }

    if (digitalRelease && moment(date).isSame(moment(digitalRelease), 'day')) {
      eventType.push('Digital');
    }

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
                  title={translate('MovieIsDownloading')}
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
                  title={translate('QualityCutoffHasNotBeenMet')}
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
                  {eventType.join(', ')}
                </div>
                <div>
                  {certification}
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
  isAvailable: PropTypes.bool.isRequired,
  inCinemas: PropTypes.string,
  physicalRelease: PropTypes.string,
  digitalRelease: PropTypes.string,
  monitored: PropTypes.bool.isRequired,
  certification: PropTypes.string,
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
