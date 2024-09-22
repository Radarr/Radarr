import PropTypes from 'prop-types';
import React from 'react';
import MovieMinimumAvailabilityPopoverContent from 'AddMovie/MovieMinimumAvailabilityPopoverContent';
import Icon from 'Components/Icon';
import VirtualTableHeader from 'Components/Table/VirtualTableHeader';
import VirtualTableHeaderCell from 'Components/Table/VirtualTableHeaderCell';
import VirtualTableSelectAllHeaderCell from 'Components/Table/VirtualTableSelectAllHeaderCell';
import Popover from 'Components/Tooltip/Popover';
import { icons, tooltipPositions } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
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
        {translate('Folder')}
      </VirtualTableHeaderCell>

      <VirtualTableHeaderCell
        className={styles.movie}
        name="movie"
      >
        {translate('Movie')}
      </VirtualTableHeaderCell>

      <VirtualTableHeaderCell
        className={styles.monitor}
        name="monitor"
      >
        {translate('Monitor')}
      </VirtualTableHeaderCell>

      <VirtualTableHeaderCell
        className={styles.minimumAvailability}
        name="minimumAvailability"
      >
        {translate('MinimumAvailability')}

        <Popover
          anchor={
            <Icon
              className={styles.detailsIcon}
              name={icons.INFO}
            />
          }
          title={translate('MinimumAvailability')}
          body={<MovieMinimumAvailabilityPopoverContent />}
          position={tooltipPositions.LEFT}
        />
      </VirtualTableHeaderCell>

      <VirtualTableHeaderCell
        className={styles.qualityProfile}
        name="qualityProfileId"
      >
        {translate('QualityProfile')}
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
