import moment from 'moment';
import PropTypes from 'prop-types';
import React from 'react';
import AgendaEventConnector from './AgendaEventConnector';
import styles from './Agenda.css';

function Agenda(props) {
  const {
    items
  } = props;

  return (
    <div className={styles.agenda}>
      {
        items.map((item, index) => {
          const momentDate = moment(item.inCinemas);
          const showDate = index === 0 ||
            !moment(items[index - 1].inCinemas).isSame(momentDate, 'day');

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
  items: PropTypes.arrayOf(PropTypes.object).isRequired
};

export default Agenda;
