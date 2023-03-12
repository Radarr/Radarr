import React, { Component } from 'react';
import Icon from 'Components/Icon';
import VirtualTableRowCell from 'Components/Table/Cells/TableRowCell';
import { icons } from 'Helpers/Props';
import { getMovieStatusDetails } from 'Movie/MovieStatus';
import translate from 'Utilities/String/translate';
import styles from './MovieStatusCell.css';

interface MovieStatusCellProps {
  className: string;
  monitored: boolean;
  status: string;
  component?: React.ElementType;
}

function MovieStatusCell(props: MovieStatusCellProps) {
  const {
    className,
    monitored,
    status,
    component: Component = VirtualTableRowCell,
    ...otherProps
  } = props;

  const statusDetails = getMovieStatusDetails(status);

  return (
    <Component className={className} {...otherProps}>
      <Icon
        className={styles.statusIcon}
        name={monitored ? icons.MONITORED : icons.UNMONITORED}
        title={
          monitored
            ? translate('MovieIsMonitored')
            : translate('MovieIsUnmonitored')
        }
      />

      <Icon
        className={styles.statusIcon}
        name={statusDetails.icon}
        title={`${statusDetails.title}: ${statusDetails.message}`}
      />
    </Component>
  );
}

export default MovieStatusCell;
