import moment from 'moment';
import PropTypes from 'prop-types';
import React from 'react';
import AgendaEventConnector from './AgendaEventConnector';
import styles from './Agenda.css';

function Agenda(props) {
  const {
    items,
    start,
    end
  } = props;

  const startDateParsed = Date.parse(start);
  const endDateParsed = Date.parse(end);

  items.forEach((item) => {
    const cinemaDateParsed = Date.parse(item.inCinemas);
    const digitalDateParsed = Date.parse(item.digitalRelease);
    const physicalDateParsed = Date.parse(item.physicalRelease);
    const dates = [];

    if (cinemaDateParsed > 0 && cinemaDateParsed >= startDateParsed && cinemaDateParsed <= endDateParsed) {
      dates.push(cinemaDateParsed);
    }
    if (digitalDateParsed > 0 && digitalDateParsed >= startDateParsed && digitalDateParsed <= endDateParsed) {
      dates.push(digitalDateParsed);
    }
    if (physicalDateParsed > 0 && physicalDateParsed >= startDateParsed && physicalDateParsed <= endDateParsed) {
      dates.push(physicalDateParsed);
    }

    item.sortDate = Math.min(...dates);
    item.cinemaDateParsed = cinemaDateParsed;
    item.digitalDateParsed = digitalDateParsed;
    item.physicalDateParsed = physicalDateParsed;
  });

  items.sort((a, b) => ((a.sortDate > b.sortDate) ? 1 : -1));

  return (
    <div className={styles.agenda}>
      {
        items.map((item, index) => {
          const momentDate = moment(item.sortDate);
          const showDate = index === 0 ||
            !moment(items[index - 1].sortDate).isSame(momentDate, 'day');

          return (
            <AgendaEventConnector
              key={item.id}
              movieId={item.id}
              showDate={showDate}
              {...item}
            />
          );
        })
      }
    </div>
  );
}

Agenda.propTypes = {
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  start: PropTypes.string.isRequired,
  end: PropTypes.string.isRequired
};

export default Agenda;
