import PropTypes from 'prop-types';
import React from 'react';
import { icons, tooltipPositions } from 'Helpers/Props';
import Icon from 'Components/Icon';
import Popover from 'Components/Tooltip/Popover';
import VirtualTableHeader from 'Components/Table/VirtualTableHeader';
import VirtualTableHeaderCell from 'Components/Table/VirtualTableHeaderCell';
import VirtualTableSelectAllHeaderCell from 'Components/Table/VirtualTableSelectAllHeaderCell';
import ArtistMonitoringOptionsPopoverContent from 'AddArtist/ArtistMonitoringOptionsPopoverContent';
// import SeriesTypePopoverContent from 'AddArtist/SeriesTypePopoverContent';
import styles from './ImportArtistHeader.css';

function ImportArtistHeader(props) {
  const {
    showMetadataProfile,
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

        <Popover
          anchor={
            <Icon
              className={styles.detailsIcon}
              name={icons.INFO}
            />
          }
          title="Monitoring Options"
          body={<ArtistMonitoringOptionsPopoverContent />}
          position={tooltipPositions.RIGHT}
        />
      </VirtualTableHeaderCell>

      <VirtualTableHeaderCell
        className={styles.qualityProfile}
        name="qualityProfileId"
      >
        Quality Profile
      </VirtualTableHeaderCell>

      {
        showMetadataProfile &&
          <VirtualTableHeaderCell
            className={styles.metadataProfile}
            name="metadataProfileId"
          >
            Metadata Profile
          </VirtualTableHeaderCell>
      }

      <VirtualTableHeaderCell
        className={styles.albumFolder}
        name="albumFolder"
      >
        Album Folder
      </VirtualTableHeaderCell>

      <VirtualTableHeaderCell
        className={styles.artist}
        name="artist"
      >
        Artist
      </VirtualTableHeaderCell>
    </VirtualTableHeader>
  );
}

ImportArtistHeader.propTypes = {
  showMetadataProfile: PropTypes.bool.isRequired,
  allSelected: PropTypes.bool.isRequired,
  allUnselected: PropTypes.bool.isRequired,
  onSelectAllChange: PropTypes.func.isRequired
};

export default ImportArtistHeader;
