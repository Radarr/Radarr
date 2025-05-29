import classNames from 'classnames';
import React from 'react';
import { CalendarStatus } from 'typings/Calendar';
import styles from './LegendItem.css';

interface LegendItemProps {
  name: string;
  status: CalendarStatus;
  isAgendaView: boolean;
  fullColorEvents: boolean;
  colorImpairedMode: boolean;
}

function LegendItem({
  name,
  status,
  isAgendaView,
  fullColorEvents,
  colorImpairedMode,
}: LegendItemProps) {
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

export default LegendItem;
