import React, { useCallback } from 'react';
import { useDispatch } from 'react-redux';
import Icon from 'Components/Icon';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import VirtualTableRowCell from 'Components/Table/Cells/TableRowCell';
import { icons } from 'Helpers/Props';
import getMovieStatusDetails from 'Movie/getMovieStatusDetails';
import { MovieStatus } from 'Movie/Movie';
import { toggleMovieMonitored } from 'Store/Actions/movieActions';
import translate from 'Utilities/String/translate';
import styles from './MovieStatusCell.css';

interface MovieStatusCellProps {
  className: string;
  movieId: number;
  monitored: boolean;
  status: MovieStatus;
  isSelectMode: boolean;
  isSaving: boolean;
  component?: React.ElementType;
}

function MovieStatusCell(props: MovieStatusCellProps) {
  const {
    className,
    movieId,
    monitored,
    status,
    isSelectMode,
    isSaving,
    component: Component = VirtualTableRowCell,
    ...otherProps
  } = props;

  const statusDetails = getMovieStatusDetails(status);

  const dispatch = useDispatch();

  const onMonitoredPress = useCallback(() => {
    dispatch(toggleMovieMonitored({ movieId, monitored: !monitored }));
  }, [movieId, monitored, dispatch]);

  return (
    <Component className={className} {...otherProps}>
      {isSelectMode ? (
        <MonitorToggleButton
          className={styles.statusIcon}
          monitored={monitored}
          isSaving={isSaving}
          onPress={onMonitoredPress}
        />
      ) : (
        <Icon
          className={styles.statusIcon}
          name={monitored ? icons.MONITORED : icons.UNMONITORED}
          title={
            monitored
              ? translate('MovieIsMonitored')
              : translate('MovieIsUnmonitored')
          }
        />
      )}

      <Icon
        className={styles.statusIcon}
        name={statusDetails.icon}
        title={`${statusDetails.title}: ${statusDetails.message}`}
      />
    </Component>
  );
}

export default MovieStatusCell;
