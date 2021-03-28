import classNames from 'classnames';
import PropTypes from 'prop-types';
import React from 'react';
import styles from './LegendItem.css';

function LegendItem(props) {
  const {
    name,
    status,
    isAgendaView,
    fullColorEvents,
    colorImpairedMode
  } = props;

  return (
    <div
      className={classNames(
        styles.legendItem,
        styles[status],
        colorImpairedMode && 'colorImpaired',
        fullColorEvents && !isAgendaView && 'fullColor'
      )}
    >
      {name}
    </div>
  );
}

LegendItem.propTypes = {
  name: PropTypes.string.isRequired,
  status: PropTypes.string.isRequired,
  isAgendaView: PropTypes.bool.isRequired,
  fullColorEvents: PropTypes.bool.isRequired,
  colorImpairedMode: PropTypes.bool.isRequired
};

export default LegendItem;
