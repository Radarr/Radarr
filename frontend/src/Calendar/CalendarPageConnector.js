import moment from 'moment';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import withCurrentPage from 'Components/withCurrentPage';
import { searchMissing, setCalendarDaysCount, setCalendarFilter } from 'Store/Actions/calendarActions';
import createAuthorCountSelector from 'Store/Selectors/createAuthorCountSelector';
import createCommandsSelector from 'Store/Selectors/createCommandsSelector';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import { isCommandExecuting } from 'Utilities/Command';
import isBefore from 'Utilities/Date/isBefore';
import CalendarPage from './CalendarPage';

function createMissingBookIdsSelector() {
  return createSelector(
    (state) => state.calendar.start,
    (state) => state.calendar.end,
    (state) => state.calendar.items,
    (state) => state.queue.details.items,
    (start, end, books, queueDetails) => {
      return books.reduce((acc, book) => {
        const releaseDate = book.releaseDate;

        if (
          book.percentOfBooks < 100 &&
          moment(releaseDate).isAfter(start) &&
          moment(releaseDate).isBefore(end) &&
          isBefore(book.releaseDate) &&
          !queueDetails.some((details) => !!details.book && details.book.id === book.id)
        ) {
          acc.push(book.id);
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
    createAuthorCountSelector(),
    createUISettingsSelector(),
    createMissingBookIdsSelector(),
    createIsSearchingSelector(),
    (
      selectedFilterKey,
      filters,
      authorCount,
      uiSettings,
      missingBookIds,
      isSearchingForMissing
    ) => {
      return {
        selectedFilterKey,
        filters,
        colorImpairedMode: uiSettings.enableColorImpairedMode,
        hasAuthor: !!authorCount.count,
        authorError: authorCount.error,
        authorIsFetching: authorCount.isFetching,
        authorIsPopulated: authorCount.isPopulated,
        missingBookIds,
        isSearchingForMissing
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onSearchMissingPress(bookIds) {
      dispatch(searchMissing({ bookIds }));
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
