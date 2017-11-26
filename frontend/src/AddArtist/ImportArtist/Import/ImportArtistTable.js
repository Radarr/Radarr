import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import VirtualTable from 'Components/Table/VirtualTable';
import ImportArtistHeader from './ImportArtistHeader';
import ImportArtistRowConnector from './ImportArtistRowConnector';

class ImportArtistTable extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this._table = null;
  }

  componentDidMount() {
    const {
      unmappedFolders,
      defaultMonitor,
      defaultQualityProfileId,
      defaultLanguageProfileId,
      defaultMetadataProfileId,
      defaultAlbumFolder,
      onArtistLookup,
      onSetImportArtistValue
    } = this.props;

    const values = {
      monitor: defaultMonitor,
      qualityProfileId: defaultQualityProfileId,
      languageProfileId: defaultLanguageProfileId,
      metadataProfileId: defaultMetadataProfileId,
      albumFolder: defaultAlbumFolder
    };

    unmappedFolders.forEach((unmappedFolder) => {
      const id = unmappedFolder.name;

      onArtistLookup(id, unmappedFolder.path);

      onSetImportArtistValue({
        id,
        ...values
      });
    });
  }

  // This isn't great, but it's the most reliable way to ensure the items
  // are checked off even if they aren't actually visible since the cells
  // are virtualized.

  componentDidUpdate(prevProps) {
    const {
      items,
      selectedState,
      onSelectedChange,
      onRemoveSelectedStateItem
    } = this.props;

    prevProps.items.forEach((prevItem) => {
      const {
        id
      } = prevItem;

      const item = _.find(items, { id });

      if (!item) {
        onRemoveSelectedStateItem(id);
        return;
      }

      const selectedArtist = item.selectedArtist;
      const isSelected = selectedState[id];

      const isExistingArtist = !!selectedArtist &&
        _.some(prevProps.allArtists, { foreignArtistId: selectedArtist.foreignArtistId });

      // Props doesn't have a selected artist or
      // the selected artist is an existing artist.
      if ((!selectedArtist && prevItem.selectedArtist) || (isExistingArtist && !prevItem.selectedArtist)) {
        onSelectedChange({ id, value: false });

        return;
      }

      // State is selected, but a artist isn't selected or
      // the selected artist is an existing artist.
      if (isSelected && (!selectedArtist || isExistingArtist)) {
        onSelectedChange({ id, value: false });

        return;
      }

      // A artist is being selected that wasn't previously selected.
      if (selectedArtist && selectedArtist !== prevItem.selectedArtist) {
        onSelectedChange({ id, value: true });

        return;
      }
    });

    // Forces the table to re-render if the selected state
    // has changed otherwise it will be stale.

    if (prevProps.selectedState !== selectedState && this._table) {
      this._table.forceUpdateGrid();
    }
  }

  //
  // Control

  setTableRef = (ref) => {
    this._table = ref;
  }

  rowRenderer = ({ key, rowIndex, style }) => {
    const {
      rootFolderId,
      items,
      selectedState,
      showLanguageProfile,
      showMetadataProfile,
      onSelectedChange
    } = this.props;

    const item = items[rowIndex];

    return (
      <ImportArtistRowConnector
        key={key}
        style={style}
        rootFolderId={rootFolderId}
        showLanguageProfile={showLanguageProfile}
        showMetadataProfile={showMetadataProfile}
        isSelected={selectedState[item.id]}
        onSelectedChange={onSelectedChange}
        id={item.id}
      />
    );
  }

  //
  // Render

  render() {
    const {
      items,
      allSelected,
      allUnselected,
      isSmallScreen,
      contentBody,
      showLanguageProfile,
      showMetadataProfile,
      scrollTop,
      onSelectAllChange,
      onScroll
    } = this.props;

    if (!items.length) {
      return null;
    }

    return (
      <VirtualTable
        ref={this.setTableRef}
        items={items}
        contentBody={contentBody}
        isSmallScreen={isSmallScreen}
        rowHeight={52}
        scrollTop={scrollTop}
        overscanRowCount={2}
        rowRenderer={this.rowRenderer}
        header={
          <ImportArtistHeader
            showLanguageProfile={showLanguageProfile}
            showMetadataProfile={showMetadataProfile}
            allSelected={allSelected}
            allUnselected={allUnselected}
            onSelectAllChange={onSelectAllChange}
          />
        }
        onScroll={onScroll}
      />
    );
  }
}

ImportArtistTable.propTypes = {
  rootFolderId: PropTypes.number.isRequired,
  items: PropTypes.arrayOf(PropTypes.object),
  unmappedFolders: PropTypes.arrayOf(PropTypes.object),
  defaultMonitor: PropTypes.string.isRequired,
  defaultQualityProfileId: PropTypes.number,
  defaultLanguageProfileId: PropTypes.number,
  defaultMetadataProfileId: PropTypes.number,
  defaultAlbumFolder: PropTypes.bool.isRequired,
  allSelected: PropTypes.bool.isRequired,
  allUnselected: PropTypes.bool.isRequired,
  selectedState: PropTypes.object.isRequired,
  isSmallScreen: PropTypes.bool.isRequired,
  allArtists: PropTypes.arrayOf(PropTypes.object),
  contentBody: PropTypes.object.isRequired,
  showLanguageProfile: PropTypes.bool.isRequired,
  showMetadataProfile: PropTypes.bool.isRequired,
  scrollTop: PropTypes.number.isRequired,
  onSelectAllChange: PropTypes.func.isRequired,
  onSelectedChange: PropTypes.func.isRequired,
  onRemoveSelectedStateItem: PropTypes.func.isRequired,
  onArtistLookup: PropTypes.func.isRequired,
  onSetImportArtistValue: PropTypes.func.isRequired,
  onScroll: PropTypes.func.isRequired
};

export default ImportArtistTable;
