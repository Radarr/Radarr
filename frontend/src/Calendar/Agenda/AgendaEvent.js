import classNames from 'classnames';
import moment from 'moment';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import CalendarEventQueueDetails from 'Calendar/Events/CalendarEventQueueDetails';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import { icons, kinds } from 'Helpers/Props';
import getStatusStyle from 'Utilities/Movie/getStatusStyle';
import translate from 'Utilities/String/translate';
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
  };

  onDetailsModalClose = () => {
    this.setState({ isDetailsModalOpen: false });
  };

  //
  // Render

  render() {
    const {
      movieFile,
      title,
      titleSlug,
      genres,
      isAvailable,
      inCinemas,
      digitalRelease,
      physicalRelease,
      monitored,
      hasFile,
      grabbed,
      queueItem,
      showDate,
      showMovieInformation,
      showCutoffUnmetIcon,
      longDateFormat,
      colorImpairedMode,
      cinemaDateParsed,
      digitalDateParsed,
      physicalDateParsed,
      sortDate
    } = this.props;

    let startTime = null;
    let releaseIcon = null;

    if (physicalDateParsed === sortDate) {
      startTime = physicalRelease;
      releaseIcon = icons.DISC;
    }

    if (digitalDateParsed === sortDate) {
      startTime = digitalRelease;
      releaseIcon = icons.MOVIE_FILE;
    }

    if (cinemaDateParsed === sortDate) {
      startTime = inCinemas;
      releaseIcon = icons.IN_CINEMAS;
    }

    startTime = moment(startTime);
    const downloading = !!(queueItem || grabbed);
    const isMonitored = monitored;
    const statusStyle = getStatusStyle(null, isMonitored, hasFile, isAvailable, 'style', downloading);
    const joinedGenres = genres.slice(0, 2).join(', ');
    const link = `/movie/${titleSlug}`;

    return (
      <div>
        <Link
          className={classNames(
            styles.event,
            styles.link
          )}
          to={link}
        >
          <div className={styles.dateIcon}>
            <Icon
              name={releaseIcon}
              kind={kinds.DEFAULT}
            />
          </div>

          <div className={styles.date}>
            {(showDate) ? startTime.format(longDateFormat) : null}
          </div>

          <div
            className={classNames(
              styles.eventWrapper,
              styles[statusStyle],
              colorImpairedMode && 'colorImpaired'
            )}
          >
            <div className={styles.movieTitle}>
              {title}
            </div>

            {
              showMovieInformation &&
                <div className={styles.genres}>
                  {joinedGenres}
                </div>
            }

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
        </Link>
      </div>
    );
  }
}

AgendaEvent.propTypes = {
  id: PropTypes.number.isRequired,
  movieFile: PropTypes.object,
  title: PropTypes.string.isRequired,
  titleSlug: PropTypes.string.isRequired,
  genres: PropTypes.arrayOf(PropTypes.string).isRequired,
  isAvailable: PropTypes.bool.isRequired,
  inCinemas: PropTypes.string,
  digitalRelease: PropTypes.string,
  physicalRelease: PropTypes.string,
  monitored: PropTypes.bool.isRequired,
  hasFile: PropTypes.bool.isRequired,
  grabbed: PropTypes.bool,
  queueItem: PropTypes.object,
  showDate: PropTypes.bool.isRequired,
  showMovieInformation: PropTypes.bool.isRequired,
  showCutoffUnmetIcon: PropTypes.bool.isRequired,
  timeFormat: PropTypes.string.isRequired,
  longDateFormat: PropTypes.string.isRequired,
  colorImpairedMode: PropTypes.bool.isRequired,
  cinemaDateParsed: PropTypes.number,
  digitalDateParsed: PropTypes.number,
  physicalDateParsed: PropTypes.number,
  sortDate: PropTypes.number
};

AgendaEvent.defaultProps = {
  genres: []
};

export default AgendaEvent;
