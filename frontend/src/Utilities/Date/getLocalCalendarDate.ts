import moment from 'moment';

// Release dates are calendar dates without a time of day, and the API sends
// them as midnight UTC. Keep the same calendar date and return it as midnight
// local time. Otherwise the date moves to the previous day in time zones
// behind UTC.
function getLocalCalendarDate(date?: string) {
  if (!date) {
    return date;
  }

  return moment.utc(date).local(true).toISOString();
}

export default getLocalCalendarDate;
