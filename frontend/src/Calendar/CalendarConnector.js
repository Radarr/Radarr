import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { clearBookFiles, fetchBookFiles } from 'Store/Actions/bookFileActions';
import * as calendarActions from 'Store/Actions/calendarActions';
import { clearQueueDetails, fetchQueueDetails } from 'Store/Actions/queueActions';
import hasDifferentItems from 'Utilities/Object/hasDifferentItems';
import selectUniqueIds from 'Utilities/Object/selectUniqueIds';
import { registerPagePopulator, unregisterPagePopulator } from 'Utilities/pagePopulator';
import Calendar from './Calendar';

const UPDATE_DELAY = 3600000; // 1 hour

function createMapStateToProps() {
  return createSelector(
    (state) => state.calendar,
    (calendar) => {
      return calendar;
    }
  );
}

const mapDispatchToProps = {
  ...calendarActions,
  fetchBookFiles,
  clearBookFiles,
  fetchQueueDetails,
  clearQueueDetails
};

class CalendarConnector extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.updateTimeoutId = null;
  }

  componentDidMount() {
    const {
      useCurrentPage,
      fetchCalendar,
      gotoCalendarToday
    } = this.props;

    registerPagePopulator(this.repopulate);

    if (useCurrentPage) {
      fetchCalendar();
    } else {
      gotoCalendarToday();
    }

    this.scheduleUpdate();
  }

  componentDidUpdate(prevProps) {
    const {
      items,
      time
    } = this.props;

    if (hasDifferentItems(prevProps.items, items)) {
      const bookIds = selectUniqueIds(items, 'id');
      // const bookFileIds = selectUniqueIds(items, 'bookFileId');

      if (items.length) {
        this.props.fetchQueueDetails({ bookIds });
      }

      // if (bookFileIds.length) {
      //   this.props.fetchBookFiles({ bookFileIds });
      // }
    }

    if (prevProps.time !== time) {
      this.scheduleUpdate();
    }
  }

  componentWillUnmount() {
    unregisterPagePopulator(this.repopulate);
    this.props.clearCalendar();
    this.props.clearQueueDetails();
    this.props.clearBookFiles();
    this.clearUpdateTimeout();
  }

  //
  // Control
  repopulate = () => {
    const {
      time,
      view
    } = this.props;

    this.props.fetchQueueDetails({ time, view });
    this.props.fetchCalendar({ time, view });
  }

  scheduleUpdate = () => {
    this.clearUpdateTimeout();

    this.updateTimeoutId = setTimeout(this.updateCalendar, UPDATE_DELAY);
  }

  clearUpdateTimeout = () => {
    if (this.updateTimeoutId) {
      clearTimeout(this.updateTimeoutId);
    }
  }

  updateCalendar = () => {
    this.props.gotoCalendarToday();
    this.scheduleUpdate();
  }

  //
  // Listeners

  onCalendarViewChange = (view) => {
    this.props.setCalendarView({ view });
  }

  onTodayPress = () => {
    this.props.gotoCalendarToday();
  }

  onPreviousPress = () => {
    this.props.gotoCalendarPreviousRange();
  }

  onNextPress = () => {
    this.props.gotoCalendarNextRange();
  }

  //
  // Render

  render() {
    return (
      <Calendar
        {...this.props}
        onCalendarViewChange={this.onCalendarViewChange}
        onTodayPress={this.onTodayPress}
        onPreviousPress={this.onPreviousPress}
        onNextPress={this.onNextPress}
      />
    );
  }
}

CalendarConnector.propTypes = {
  useCurrentPage: PropTypes.bool.isRequired,
  time: PropTypes.string,
  view: PropTypes.string.isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  setCalendarView: PropTypes.func.isRequired,
  gotoCalendarToday: PropTypes.func.isRequired,
  gotoCalendarPreviousRange: PropTypes.func.isRequired,
  gotoCalendarNextRange: PropTypes.func.isRequired,
  clearCalendar: PropTypes.func.isRequired,
  fetchCalendar: PropTypes.func.isRequired,
  fetchBookFiles: PropTypes.func.isRequired,
  clearBookFiles: PropTypes.func.isRequired,
  fetchQueueDetails: PropTypes.func.isRequired,
  clearQueueDetails: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(CalendarConnector);
