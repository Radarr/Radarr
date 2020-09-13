import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createMovieFileSelector from 'Store/Selectors/createMovieFileSelector';
import createMovieSelector from 'Store/Selectors/createMovieSelector';
import createQueueItemSelector from 'Store/Selectors/createQueueItemSelector';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import AgendaEvent from './AgendaEvent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.calendar.options,
    createMovieSelector(),
    createMovieFileSelector(),
    createQueueItemSelector(),
    createUISettingsSelector(),
    (state) => state.calendar.start,
    (state) => state.calendar.end,
    (calendarOptions, movie, movieFile, queueItem, uiSettings, startDate, endDate) => {
      return {
        movie,
        movieFile,
        queueItem,
        ...calendarOptions,
        timeFormat: uiSettings.timeFormat,
        longDateFormat: uiSettings.longDateFormat,
        colorImpairedMode: uiSettings.enableColorImpairedMode,
        startDate,
        endDate
      };
    }
  );
}

export default connect(createMapStateToProps)(AgendaEvent);
