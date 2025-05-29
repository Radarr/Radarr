import moment from 'moment';
import React, { useMemo } from 'react';
import { useSelector } from 'react-redux';
import AppState from 'App/State/AppState';
import Movie from 'Movie/Movie';
import AgendaEvent from './AgendaEvent';
import styles from './Agenda.css';

interface AgendaMovie extends Movie {
  sortDate: moment.Moment;
}

function Agenda() {
  const { start, end, items } = useSelector(
    (state: AppState) => state.calendar
  );

  const events = useMemo(() => {
    const result = items.map((item): AgendaMovie => {
      const { inCinemas, digitalRelease, physicalRelease } = item;

      const dates = [];

      if (inCinemas) {
        const inCinemasMoment = moment(inCinemas);

        if (inCinemasMoment.isAfter(start) && inCinemasMoment.isBefore(end)) {
          dates.push(inCinemasMoment);
        }
      }

      if (digitalRelease) {
        const digitalReleaseMoment = moment(digitalRelease);

        if (
          digitalReleaseMoment.isAfter(start) &&
          digitalReleaseMoment.isBefore(end)
        ) {
          dates.push(digitalReleaseMoment);
        }
      }

      if (physicalRelease) {
        const physicalReleaseMoment = moment(physicalRelease);

        if (
          physicalReleaseMoment.isAfter(start) &&
          physicalReleaseMoment.isBefore(end)
        ) {
          dates.push(physicalReleaseMoment);
        }
      }

      const sortDate = moment.min(...dates);

      return {
        ...item,
        sortDate,
      };
    });

    result.sort((a, b) => (a.sortDate > b.sortDate ? 1 : -1));

    return result;
  }, [items, start, end]);

  return (
    <div className={styles.agenda}>
      {events.map((item, index) => {
        const momentDate = moment(item.sortDate);
        const showDate =
          index === 0 ||
          !moment(events[index - 1].sortDate).isSame(momentDate, 'day');

        return <AgendaEvent key={item.id} showDate={showDate} {...item} />;
      })}
    </div>
  );
}

export default Agenda;
