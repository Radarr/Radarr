import PropTypes from 'prop-types';
import React from 'react';
import MonitorToggleButton from 'Components/MonitorToggleButton';
import styles from './MovieCollection.css';

function MovieCollection(props) {
  const {
    name,
    collectionList,
    isSaving,
    onMonitorTogglePress
  } = props;

  const monitored = collectionList !== undefined && collectionList.enabled && collectionList.enableAuto;

  return (
    <div>
      <MonitorToggleButton
        className={styles.monitorToggleButton}
        monitored={monitored}
        isSaving={isSaving}
        size={15}
        onPress={onMonitorTogglePress}
      />
      {name}
    </div>
  );
}

MovieCollection.propTypes = {
  tmdbId: PropTypes.number.isRequired,
  name: PropTypes.string.isRequired,
  collectionList: PropTypes.object,
  isSaving: PropTypes.bool.isRequired,
  onMonitorTogglePress: PropTypes.func.isRequired
};

export default MovieCollection;
