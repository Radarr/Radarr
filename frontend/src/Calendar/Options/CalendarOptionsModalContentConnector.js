import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchCalendar, setCalendarOption } from 'Store/Actions/calendarActions';
import { saveUISettings } from 'Store/Actions/settingsActions';
import CalendarOptionsModalContent from './CalendarOptionsModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.calendar.options,
    (state) => state.settings.ui.item,
    (options, uiSettings) => {
      return {
        ...options,
        ...uiSettings
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchSetCalendarOption: setCalendarOption,
  dispatchSaveUISettings: saveUISettings,
  fetchCalendar
};

export default connect(createMapStateToProps, mapDispatchToProps)(CalendarOptionsModalContent);
