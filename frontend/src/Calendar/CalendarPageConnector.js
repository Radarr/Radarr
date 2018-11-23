import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { setCalendarDaysCount, setCalendarFilter } from 'Store/Actions/calendarActions';
import createMovieCountSelector from 'Store/Selectors/createMovieCountSelector';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import CalendarPage from './CalendarPage';

function createMapStateToProps() {
  return createSelector(
    (state) => state.calendar,
    createMovieCountSelector(),
    createUISettingsSelector(),
    (calendar, seriesCount, uiSettings) => {
      return {
        selectedFilterKey: calendar.selectedFilterKey,
        filters: calendar.filters,
        colorImpairedMode: uiSettings.enableColorImpairedMode,
        hasSeries: !!seriesCount
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    onDaysCountChange(dayCount) {
      dispatch(setCalendarDaysCount({ dayCount }));
    },

    onFilterSelect(selectedFilterKey) {
      dispatch(setCalendarFilter({ selectedFilterKey }));
    }
  };
}

export default connect(createMapStateToProps, createMapDispatchToProps)(CalendarPage);
