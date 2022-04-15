import PropTypes from 'prop-types';
import React from 'react';
import FormInputGroup from 'Components/Form/FormInputGroup';
import VirtualTableRowCell from 'Components/Table/Cells/VirtualTableRowCell';
import VirtualTableSelectCell from 'Components/Table/Cells/VirtualTableSelectCell';
import { inputTypes } from 'Helpers/Props';
import ImportMovieSelectMovieConnector from './SelectMovie/ImportMovieSelectMovieConnector';
import styles from './ImportMovieRow.css';

function ImportMovieRow(props) {
  const {
    id,
    monitor,
    qualityProfileIds,
    minimumAvailability,
    selectedMovie,
    isExistingMovie,
    isSelected,
    onSelectedChange,
    onInputChange
  } = props;

  return (
    <>
      <VirtualTableSelectCell
        inputClassName={styles.selectInput}
        id={id}
        isSelected={isSelected}
        isDisabled={!selectedMovie || isExistingMovie}
        onSelectedChange={onSelectedChange}
      />

      <VirtualTableRowCell className={styles.folder}>
        {id}
      </VirtualTableRowCell>

      <VirtualTableRowCell className={styles.movie}>
        <ImportMovieSelectMovieConnector
          id={id}
          isExistingMovie={isExistingMovie}
        />
      </VirtualTableRowCell>

      <VirtualTableRowCell className={styles.monitor}>
        <FormInputGroup
          type={inputTypes.MOVIE_MONITORED_SELECT}
          name="monitor"
          value={monitor}
          onChange={onInputChange}
        />
      </VirtualTableRowCell>

      <VirtualTableRowCell className={styles.minimumAvailability}>
        <FormInputGroup
          type={inputTypes.AVAILABILITY_SELECT}
          name="minimumAvailability"
          value={minimumAvailability}
          onChange={onInputChange}
        />
      </VirtualTableRowCell>

      <VirtualTableRowCell className={styles.qualityProfile}>
        <FormInputGroup
          type={inputTypes.QUALITY_PROFILE_SELECT}
          name="qualityProfileIds"
          value={qualityProfileIds}
          onChange={onInputChange}
        />
      </VirtualTableRowCell>
    </>
  );
}

ImportMovieRow.propTypes = {
  id: PropTypes.string.isRequired,
  monitor: PropTypes.string.isRequired,
  qualityProfileIds: PropTypes.arrayOf(PropTypes.number).isRequired,
  minimumAvailability: PropTypes.string.isRequired,
  selectedMovie: PropTypes.object,
  isExistingMovie: PropTypes.bool.isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  queued: PropTypes.bool.isRequired,
  isSelected: PropTypes.bool,
  onSelectedChange: PropTypes.func.isRequired,
  onInputChange: PropTypes.func.isRequired
};

ImportMovieRow.defaultsProps = {
  items: []
};

export default ImportMovieRow;
