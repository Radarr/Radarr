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
    (calendarOptions, series, episodeFile, queueItem, uiSettings) => {
      return {
        series,
        episodeFile,
        queueItem,
        ...calendarOptions,
        timeFormat: uiSettings.timeFormat,
        longDateFormat: uiSettings.longDateFormat,
        colorImpairedMode: uiSettings.enableColorImpairedMode
      };
    }
  );
}

export default connect(createMapStateToProps)(AgendaEvent);
