import PropTypes from 'prop-types';
import React from 'react';
import { inputTypes } from 'Helpers/Props';
import FormInputGroup from 'Components/Form/FormInputGroup';
import VirtualTableRow from 'Components/Table/VirtualTableRow';
import VirtualTableRowCell from 'Components/Table/Cells/VirtualTableRowCell';
import VirtualTableSelectCell from 'Components/Table/Cells/VirtualTableSelectCell';
import ImportArtistSelectArtistConnector from './SelectArtist/ImportArtistSelectArtistConnector';
import styles from './ImportArtistRow.css';

function ImportArtistRow(props) {
  const {
    style,
    id,
    monitor,
    qualityProfileId,
    metadataProfileId,
    albumFolder,
    selectedArtist,
    isExistingArtist,
    showMetadataProfile,
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
        isDisabled={!selectedArtist || isExistingArtist}
        onSelectedChange={onSelectedChange}
      />

      <VirtualTableRowCell className={styles.folder}>
        {id}
      </VirtualTableRowCell>

      <VirtualTableRowCell className={styles.monitor}>
        <FormInputGroup
          type={inputTypes.MONITOR_ALBUMS_SELECT}
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

      <VirtualTableRowCell
        className={showMetadataProfile ? styles.metadataProfile : styles.hideMetadataProfile}
      >
        <FormInputGroup
          type={inputTypes.METADATA_PROFILE_SELECT}
          name="metadataProfileId"
          value={metadataProfileId}
          onChange={onInputChange}
        />
      </VirtualTableRowCell>

      <VirtualTableRowCell className={styles.albumFolder}>
        <FormInputGroup
          type={inputTypes.CHECK}
          name="albumFolder"
          value={albumFolder}
          onChange={onInputChange}
        />
      </VirtualTableRowCell>

      <VirtualTableRowCell className={styles.artist}>
        <ImportArtistSelectArtistConnector
          id={id}
          isExistingArtist={isExistingArtist}
        />
      </VirtualTableRowCell>
    </VirtualTableRow>
  );
}

ImportArtistRow.propTypes = {
  style: PropTypes.object.isRequired,
  id: PropTypes.string.isRequired,
  monitor: PropTypes.string.isRequired,
  qualityProfileId: PropTypes.number.isRequired,
  metadataProfileId: PropTypes.number.isRequired,
  albumFolder: PropTypes.bool.isRequired,
  selectedArtist: PropTypes.object,
  isExistingArtist: PropTypes.bool.isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  showMetadataProfile: PropTypes.bool.isRequired,
  isSelected: PropTypes.bool,
  onSelectedChange: PropTypes.func.isRequired,
  onInputChange: PropTypes.func.isRequired
};

ImportArtistRow.defaultsProps = {
  items: []
};

export default ImportArtistRow;
