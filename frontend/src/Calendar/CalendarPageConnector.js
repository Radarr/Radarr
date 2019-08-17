import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import moment from 'moment';
import { isCommandExecuting } from 'Utilities/Command';
import isBefore from 'Utilities/Date/isBefore';
import withCurrentPage from 'Components/withCurrentPage';
import { searchMissing, setCalendarDaysCount, setCalendarFilter } from 'Store/Actions/calendarActions';
import createArtistCountSelector from 'Store/Selectors/createArtistCountSelector';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import createCommandsSelector from 'Store/Selectors/createCommandsSelector';
import CalendarPage from './CalendarPage';

function createMissingAlbumIdsSelector() {
  return createSelector(
    (state) => state.calendar.start,
    (state) => state.calendar.end,
    (state) => state.calendar.items,
    (state) => state.queue.details.items,
    (start, end, albums, queueDetails) => {
      return albums.reduce((acc, album) => {
        const releaseDate = album.releaseDate;

        if (
          album.percentOfTracks < 100 &&
          moment(releaseDate).isAfter(start) &&
          moment(releaseDate).isBefore(end) &&
          isBefore(album.releaseDate) &&
          !queueDetails.some((details) => !!details.album && details.album.id === album.id)
        ) {
          acc.push(album.id);
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
    createArtistCountSelector(),
    createUISettingsSelector(),
    createMissingAlbumIdsSelector(),
    createIsSearchingSelector(),
    (
      selectedFilterKey,
      filters,
      artistCount,
      uiSettings,
      missingAlbumIds,
      isSearchingForMissing
    ) => {
      return {
        selectedFilterKey,
        filters,
        colorImpairedMode: uiSettings.enableColorImpairedMode,
        hasArtist: !!artistCount.count,
        artistError: artistCount.error,
        missingAlbumIds,
        isSearchingForMissing
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onSearchMissingPress(albumIds) {
      dispatch(searchMissing({ albumIds }));
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
