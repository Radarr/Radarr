import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createMovieSelector from 'Store/Selectors/createMovieSelector';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import CalendarEventGroup from './CalendarEventGroup';

function createIsDownloadingSelector() {
  return createSelector(
    (state, { episodeIds }) => episodeIds,
    (state) => state.queue.details,
    (episodeIds, details) => {
      return details.items.some((item) => {
        return episodeIds.includes(item.episode.id);
      });
    }
  );
}

function createMapStateToProps() {
  return createSelector(
    (state) => state.calendar.options,
    createMovieSelector(),
    createIsDownloadingSelector(),
    createUISettingsSelector(),
    (calendarOptions, series, isDownloading, uiSettings) => {
      return {
        series,
        isDownloading,
        ...calendarOptions,
        timeFormat: uiSettings.timeFormat,
        colorImpairedMode: uiSettings.enableColorImpairedMode
      };
    }
  );
}

export default connect(createMapStateToProps)(CalendarEventGroup);
