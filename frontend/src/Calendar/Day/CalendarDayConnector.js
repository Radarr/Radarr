import _ from 'lodash';
import moment from 'moment';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { createSelector } from 'reselect';
import connectSection from 'Store/connectSection';
import CalendarDay from './CalendarDay';

function createCalendarEventsConnector() {
  return createSelector(
    (state, { date }) => date,
    (state) => state.calendar,
    (date, calendar) => {
      const filtered = _.filter(calendar.items, (item) => {
        return moment(date).isSame(moment(item.releaseDate), 'day');
      });

      return _.sortBy(filtered, (item) => moment(item.releaseDate).unix());
    }
  );
}

function createMapStateToProps() {
  return createSelector(
    (state) => state.calendar,
    createCalendarEventsConnector(),
    (calendar, events) => {
      return {
        time: calendar.time,
        view: calendar.view,
        events
      };
    }
  );
}

class CalendarDayConnector extends Component {

  //
  // Render

  render() {
    return (
      <CalendarDay
        {...this.props}
      />
    );
  }
}

CalendarDayConnector.propTypes = {
  date: PropTypes.string.isRequired
};

export default connectSection(
  createMapStateToProps,
  undefined,
  undefined,
  undefined,
  { section: 'calendar' }
)(CalendarDayConnector);
