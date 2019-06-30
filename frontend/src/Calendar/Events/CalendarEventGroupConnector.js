import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createMovieSelector from 'Store/Selectors/createMovieSelector';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import CalendarEventGroup from './CalendarEventGroup';

function createIsDownloadingSelector() {
  return createSelector(
    (state, { movieIds }) => movieIds,
    (state) => state.queue.details,
    (movieIds, details) => {
      return details.items.some((item) => {
        return item.movie && movieIds.includes(item.movie.id);
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
    (calendarOptions, movie, isDownloading, uiSettings) => {
      return {
        movie,
        isDownloading,
        ...calendarOptions,
        timeFormat: uiSettings.timeFormat,
        colorImpairedMode: uiSettings.enableColorImpairedMode
      };
    }
  );
}

export default connect(createMapStateToProps)(CalendarEventGroup);
