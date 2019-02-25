import moment from 'moment';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import classNames from 'classnames';
import { icons, kinds } from 'Helpers/Props';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import getStatusStyle from 'Calendar/getStatusStyle';
import CalendarEventConnector from 'Calendar/Events/CalendarEventConnector';
import styles from './CalendarEventGroup.css';

function getEventsInfo(events) {
  let files = 0;
  let queued = 0;
  let monitored = 0;
  let absoluteEpisodeNumbers = 0;

  events.forEach((event) => {
    if (event.episodeFileId) {
      files++;
    }

    if (event.queued) {
      queued++;
    }

    if (event.monitored) {
      monitored++;
    }

    if (event.absoluteEpisodeNumber) {
      absoluteEpisodeNumbers++;
    }
  });

  return {
    allDownloaded: files === events.length,
    anyQueued: queued > 0,
    anyMonitored: monitored > 0,
    allAbsoluteEpisodeNumbers: absoluteEpisodeNumbers === events.length
  };
}

class CalendarEventGroup extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isExpanded: false
    };
  }

  //
  // Listeners

  onExpandPress = () => {
    this.setState({ isExpanded: !this.state.isExpanded });
  }

  //
  // Render

  render() {
    const {
      series,
      events,
      isDownloading,
      showEpisodeInformation,
      showFinaleIcon,
      colorImpairedMode,
      onEventModalOpenToggle
    } = this.props;

    const { isExpanded } = this.state;
    const {
      allDownloaded,
      anyQueued,
      anyMonitored
    } = getEventsInfo(events);
    const anyDownloading = isDownloading || anyQueued;
    const firstEpisode = events[0];
    const lastEpisode = events[events.length -1];
    const airDateUtc = firstEpisode.airDateUtc;
    const startTime = moment(airDateUtc);
    const endTime = moment(lastEpisode.airDateUtc).add(series.runtime, 'minutes');
    const seasonNumber = firstEpisode.seasonNumber;
    const statusStyle = getStatusStyle(allDownloaded, anyDownloading, startTime, endTime, anyMonitored);

    if (isExpanded) {
      return (
        <div>
          {
            events.map((event) => {
              if (event.isGroup) {
                return null;
              }

              return (
                <CalendarEventConnector
                  key={event.id}
                  episodeId={event.id}
                  {...event}
                  onEventModalOpenToggle={onEventModalOpenToggle}
                />
              );
            })
          }

          <Link
            className={styles.collapseContainer}
            component="div"
            onPress={this.onExpandPress}
          >
            <Icon
              name={icons.COLLAPSE}
            />
          </Link>
        </div>
      );
    }

    return (
      <div
        className={classNames(
          styles.eventGroup,
          styles[statusStyle],
          colorImpairedMode && 'colorImpaired'
        )}
      >
        <div className={styles.info}>
          <div className={styles.seriesTitle}>
            {series.title}
          </div>

          {
            anyDownloading &&
              <Icon
                className={styles.statusIcon}
                name={icons.DOWNLOADING}
                title="An episode is downloading"
              />
          }

          {
            firstEpisode.episodeNumber === 1 && seasonNumber > 0 &&
              <Icon
                className={styles.statusIcon}
                name={icons.INFO}
                kind={kinds.INFO}
                title={seasonNumber === 1 ? 'Series Premiere' : 'Season Premiere'}
              />
          }

          {
            showFinaleIcon &&
            lastEpisode.episodeNumber !== 1 &&
            seasonNumber > 0 &&
            lastEpisode.episodeNumber === series.seasons.find((season) => season.seasonNumber === seasonNumber).statistics.totalEpisodeCount &&
              <Icon
                className={styles.statusIcon}
                name={icons.INFO}
                kind={kinds.WARNING}
                title={series.status === 'ended' ? 'Series finale' : 'Season finale'}
              />
          }
        </div>

        {
          showEpisodeInformation &&
            <Link
              className={styles.expandContainer}
              component="div"
              onPress={this.onExpandPress}
            >
              <Icon
                name={icons.EXPAND}
              />
            </Link>
        }
      </div>
    );
  }
}

CalendarEventGroup.propTypes = {
  series: PropTypes.object.isRequired,
  events: PropTypes.arrayOf(PropTypes.object).isRequired,
  isDownloading: PropTypes.bool.isRequired,
  showEpisodeInformation: PropTypes.bool.isRequired,
  showFinaleIcon: PropTypes.bool.isRequired,
  timeFormat: PropTypes.string.isRequired,
  colorImpairedMode: PropTypes.bool.isRequired,
  onEventModalOpenToggle: PropTypes.func.isRequired
};

export default CalendarEventGroup;
