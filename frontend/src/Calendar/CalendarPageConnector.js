import moment from 'moment';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import * as commandNames from 'Commands/commandNames';
import withCurrentPage from 'Components/withCurrentPage';
import { searchMissing, setCalendarDaysCount, setCalendarFilter } from 'Store/Actions/calendarActions';
import { executeCommand } from 'Store/Actions/commandActions';
import createCommandExecutingSelector from 'Store/Selectors/createCommandExecutingSelector';
import createCommandsSelector from 'Store/Selectors/createCommandsSelector';
import createMovieCountSelector from 'Store/Selectors/createMovieCountSelector';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import { isCommandExecuting } from 'Utilities/Command';
import isBefore from 'Utilities/Date/isBefore';
import CalendarPage from './CalendarPage';

function createMissingMovieIdsSelector() {
  return createSelector(
    (state) => state.calendar.start,
    (state) => state.calendar.end,
    (state) => state.calendar.items,
    (state) => state.queue.details.items,
    (start, end, movies, queueDetails) => {
      return movies.reduce((acc, movie) => {
        const inCinemas = movie.inCinemas;

        if (
          !movie.hasFile &&
          moment(inCinemas).isAfter(start) &&
          moment(inCinemas).isBefore(end) &&
          isBefore(movie.inCinemas) &&
          !queueDetails.some((details) => details.movieId === movie.id)
        ) {
          acc.push(movie.id);
        }

        return acc;
      }, []);
    }
  );
}

function createIsSearchingSelector() {
  return createSelector(
    (state) => state.calendar.searchMissingCommandId,
    createCommandsSelector(),
    (searchMissingCommandId, commands) => {
      if (searchMissingCommandId == null) {
        return false;
      }

      return isCommandExecuting(commands.find((command) => {
        return command.id === searchMissingCommandId;
      }));
    }
  );
}

function createMapStateToProps() {
  return createSelector(
    (state) => state.calendar.selectedFilterKey,
    (state) => state.calendar.filters,
    createMovieCountSelector(),
    createUISettingsSelector(),
    createMissingMovieIdsSelector(),
    createCommandExecutingSelector(commandNames.RSS_SYNC),
    createIsSearchingSelector(),
    (
      selectedFilterKey,
      filters,
      movieCount,
      uiSettings,
      missingMovieIds,
      isRssSyncExecuting,
      isSearchingForMissing
    ) => {
      return {
        selectedFilterKey,
        filters,
        colorImpairedMode: uiSettings.enableColorImpairedMode,
        hasMovie: !!movieCount.count,
        movieError: movieCount.error,
        movieIsFetching: movieCount.isFetching,
        movieIsPopulated: movieCount.isPopulated,
        missingMovieIds,
        isRssSyncExecuting,
        isSearchingForMissing
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onRssSyncPress() {
      dispatch(executeCommand({
        name: commandNames.RSS_SYNC
      }));
    },

    onSearchMissingPress(movieIds) {
      dispatch(searchMissing({ movieIds }));
    },

    onDaysCountChange(dayCount) {
      dispatch(setCalendarDaysCount({ dayCount }));
    },

    onFilterSelect(selectedFilterKey) {
      dispatch(setCalendarFilter({ selectedFilterKey }));
    }
  };
}

export default withCurrentPage(
  connect(createMapStateToProps, createMapDispatchToProps)(CalendarPage)
);
