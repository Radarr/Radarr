import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import moment from 'moment';
import { isCommandExecuting } from 'Utilities/Command';
import isBefore from 'Utilities/Date/isBefore';
import withCurrentPage from 'Components/withCurrentPage';
import { searchMissing, setCalendarDaysCount, setCalendarFilter } from 'Store/Actions/calendarActions';
import createMovieCountSelector from 'Store/Selectors/createMovieCountSelector';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import createCommandsSelector from 'Store/Selectors/createCommandsSelector';
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
          !movie.movieFileId &&
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
    createIsSearchingSelector(),
    (
      selectedFilterKey,
      filters,
      movieCount,
      uiSettings,
      missingMovieIds,
      isSearchingForMissing
    ) => {
      return {
        selectedFilterKey,
        filters,
        colorImpairedMode: uiSettings.enableColorImpairedMode,
        hasMovie: !!movieCount,
        missingMovieIds,
        isSearchingForMissing
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
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
