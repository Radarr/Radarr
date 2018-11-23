import PropTypes from 'prop-types';
import React from 'react';
import VirtualTableHeader from 'Components/Table/VirtualTableHeader';
import VirtualTableHeaderCell from 'Components/Table/VirtualTableHeaderCell';
import VirtualTableSelectAllHeaderCell from 'Components/Table/VirtualTableSelectAllHeaderCell';
import styles from './ImportMovieHeader.css';

function ImportMovieHeader(props) {
  const {
    allSelected,
    allUnselected,
    onSelectAllChange
  } = props;

  return (
    <VirtualTableHeader>
      <VirtualTableSelectAllHeaderCell
        allSelected={allSelected}
        allUnselected={allUnselected}
        onSelectAllChange={onSelectAllChange}
      />

      <VirtualTableHeaderCell
        className={styles.folder}
        name="folder"
      >
        Folder
      </VirtualTableHeaderCell>

      <VirtualTableHeaderCell
        className={styles.monitor}
        name="monitor"
      >
        Monitor
      </VirtualTableHeaderCell>

      <VirtualTableHeaderCell
        className={styles.qualityProfile}
        name="qualityProfileId"
      >
        Quality Profile
      </VirtualTableHeaderCell>

      <VirtualTableHeaderCell
        className={styles.movie}
        name="movie"
      >
        Movie
      </VirtualTableHeaderCell>
    </VirtualTableHeader>
  );
}

ImportMovieHeader.propTypes = {
  allSelected: PropTypes.bool.isRequired,
  allUnselected: PropTypes.bool.isRequired,
  onSelectAllChange: PropTypes.func.isRequired
};

export default ImportMovieHeader;
