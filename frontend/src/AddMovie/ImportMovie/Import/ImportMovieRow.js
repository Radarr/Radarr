import PropTypes from 'prop-types';
import React from 'react';
import { inputTypes } from 'Helpers/Props';
import FormInputGroup from 'Components/Form/FormInputGroup';
import VirtualTableRow from 'Components/Table/VirtualTableRow';
import VirtualTableRowCell from 'Components/Table/Cells/VirtualTableRowCell';
import VirtualTableSelectCell from 'Components/Table/Cells/VirtualTableSelectCell';
import ImportMovieSelectMovieConnector from './SelectMovie/ImportMovieSelectMovieConnector';
import styles from './ImportMovieRow.css';

function ImportMovieRow(props) {
  const {
    style,
    id,
    monitor,
    qualityProfileId,
    selectedMovie,
    isExistingMovie,
    isSelected,
    onSelectedChange,
    onInputChange
  } = props;

  return (
    <VirtualTableRow style={style}>
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

      <VirtualTableRowCell className={styles.monitor}>
        <FormInputGroup
          type={inputTypes.MOVIE_MONITORED_SELECT}
          name="monitor"
          value={monitor}
          onChange={onInputChange}
        />
      </VirtualTableRowCell>

      <VirtualTableRowCell className={styles.qualityProfile}>
        <FormInputGroup
          type={inputTypes.QUALITY_PROFILE_SELECT}
          name="qualityProfileId"
          value={qualityProfileId}
          onChange={onInputChange}
        />
      </VirtualTableRowCell>

      <VirtualTableRowCell className={styles.series}>
        <ImportMovieSelectMovieConnector
          id={id}
          isExistingMovie={isExistingMovie}
        />
      </VirtualTableRowCell>
    </VirtualTableRow>
  );
}

ImportMovieRow.propTypes = {
  style: PropTypes.object.isRequired,
  id: PropTypes.string.isRequired,
  monitor: PropTypes.string.isRequired,
  qualityProfileId: PropTypes.number.isRequired,
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
