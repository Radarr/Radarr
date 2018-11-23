import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import VirtualTable from 'Components/Table/VirtualTable';
import ImportMovieHeader from './ImportMovieHeader';
import ImportMovieRowConnector from './ImportMovieRowConnector';

class ImportMovieTable extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      unmappedFolders,
      defaultMonitor,
      defaultQualityProfileId,
      onMovieLookup,
      onSetImportMovieValue
    } = this.props;

    const values = {
      monitor: defaultMonitor,
      qualityProfileId: defaultQualityProfileId
    };

    unmappedFolders.forEach((unmappedFolder) => {
      const id = unmappedFolder.name;

      onMovieLookup(id, unmappedFolder.path);

      onSetImportMovieValue({
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

      const selectedMovie = item.selectedMovie;
      const isSelected = selectedState[id];

      const isExistingMovie = !!selectedMovie &&
        _.some(prevProps.allMovies, { tmdbId: selectedMovie.tmdbId });

      // Props doesn't have a selected series or
      // the selected series is an existing series.
      if ((!selectedMovie && prevItem.selectedMovie) || (isExistingMovie && !prevItem.selectedMovie)) {
        onSelectedChange({ id, value: false });

        return;
      }

      // State is selected, but a series isn't selected or
      // the selected series is an existing series.
      if (isSelected && (!selectedMovie || isExistingMovie)) {
        onSelectedChange({ id, value: false });

        return;
      }

      // A series is being selected that wasn't previously selected.
      if (selectedMovie && selectedMovie !== prevItem.selectedMovie) {
        onSelectedChange({ id, value: true });

        return;
      }
    });
  }

  //
  // Control

  rowRenderer = ({ key, rowIndex, style }) => {
    const {
      rootFolderId,
      items,
      selectedState,
      onSelectedChange
    } = this.props;

    const item = items[rowIndex];

    return (
      <ImportMovieRowConnector
        key={key}
        style={style}
        rootFolderId={rootFolderId}
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
      scrollTop,
      selectedState,
      onSelectAllChange,
      onScroll
    } = this.props;

    if (!items.length) {
      return null;
    }

    return (
      <VirtualTable
        items={items}
        contentBody={contentBody}
        isSmallScreen={isSmallScreen}
        rowHeight={52}
        scrollTop={scrollTop}
        overscanRowCount={2}
        rowRenderer={this.rowRenderer}
        header={
          <ImportMovieHeader
            allSelected={allSelected}
            allUnselected={allUnselected}
            onSelectAllChange={onSelectAllChange}
          />
        }
        selectedState={selectedState}
        onScroll={onScroll}
      />
    );
  }
}

ImportMovieTable.propTypes = {
  rootFolderId: PropTypes.number.isRequired,
  items: PropTypes.arrayOf(PropTypes.object),
  unmappedFolders: PropTypes.arrayOf(PropTypes.object),
  defaultMonitor: PropTypes.string.isRequired,
  defaultQualityProfileId: PropTypes.number,
  allSelected: PropTypes.bool.isRequired,
  allUnselected: PropTypes.bool.isRequired,
  selectedState: PropTypes.object.isRequired,
  isSmallScreen: PropTypes.bool.isRequired,
  allMovies: PropTypes.arrayOf(PropTypes.object),
  contentBody: PropTypes.object.isRequired,
  scrollTop: PropTypes.number.isRequired,
  onSelectAllChange: PropTypes.func.isRequired,
  onSelectedChange: PropTypes.func.isRequired,
  onRemoveSelectedStateItem: PropTypes.func.isRequired,
  onMovieLookup: PropTypes.func.isRequired,
  onSetImportMovieValue: PropTypes.func.isRequired,
  onScroll: PropTypes.func.isRequired
};

export default ImportMovieTable;
