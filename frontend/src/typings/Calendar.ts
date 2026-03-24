import Movie from 'Movie/Movie';

export type CalendarItem = Movie;

export type CalendarEvent = CalendarItem;

export type CalendarStatus =
  | 'downloaded'
  | 'queue'
  | 'unmonitored'
  | 'missingMonitored'
  | 'missingUnmonitored'
  | 'continuing';
