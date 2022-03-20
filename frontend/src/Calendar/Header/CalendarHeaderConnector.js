import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { gotoCalendarNextRange, gotoCalendarPreviousRange, gotoCalendarToday, setCalendarView } from 'Store/Actions/calendarActions';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import createUISettingsSelector from 'Store/Selectors/createUISettingsSelector';
import CalendarHeader from './CalendarHeader';

function createMapStateToProps() {
  return createSelector(
    (state) => state.calendar,
    createDimensionsSelector(),
    createUISettingsSelector(),
    (calendar, dimensions, uiSettings) => {
      return {
        isFetching: calendar.isFetching,
        view: calendar.view,
        time: calendar.time,
        start: calendar.start,
        end: calendar.end,
        isSmallScreen: dimensions.isSmallScreen,
        collapseViewButtons: dimensions.isLargeScreen,
        longDateFormat: uiSettings.longDateFormat
      };
    }
  );
}

const mapDispatchToProps = {
  setCalendarView,
  gotoCalendarToday,
  gotoCalendarPreviousRange,
  gotoCalendarNextRange
};

class CalendarHeaderConnector extends Component {

  //
  // Listeners

  onViewChange = (view) => {
    this.props.setCalendarView({ view });
  };

  onTodayPress = () => {
    this.props.gotoCalendarToday();
  };

  onPreviousPress = () => {
    this.props.gotoCalendarPreviousRange();
  };

  onNextPress = () => {
    this.props.gotoCalendarNextRange();
  };

  //
  // Render

  render() {
    return (
      <CalendarHeader
        {...this.props}
        onViewChange={this.onViewChange}
        onTodayPress={this.onTodayPress}
        onPreviousPress={this.onPreviousPress}
        onNextPress={this.onNextPress}
      />
    );
  }
}

CalendarHeaderConnector.propTypes = {
  setCalendarView: PropTypes.func.isRequired,
  gotoCalendarToday: PropTypes.func.isRequired,
  gotoCalendarPreviousRange: PropTypes.func.isRequired,
  gotoCalendarNextRange: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(CalendarHeaderConnector);
