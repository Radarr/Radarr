import PropTypes from 'prop-types';
import React from 'react';
import CollectionFilterModalConnector from 'Collection/CollectionFilterModalConnector';
import FilterMenu from 'Components/Menu/FilterMenu';
import { align } from 'Helpers/Props';

function CollectionFilterMenu(props) {
  const {
    selectedFilterKey,
    filters,
    customFilters,
    isDisabled,
    onFilterSelect
  } = props;

  return (
    <FilterMenu
      alignMenu={align.RIGHT}
      isDisabled={isDisabled}
      selectedFilterKey={selectedFilterKey}
      filters={filters}
      customFilters={customFilters}
      filterModalConnectorComponent={CollectionFilterModalConnector}
      onFilterSelect={onFilterSelect}
    />
  );
}

CollectionFilterMenu.propTypes = {
  selectedFilterKey: PropTypes.oneOfType([PropTypes.string, PropTypes.number]).isRequired,
  filters: PropTypes.arrayOf(PropTypes.object).isRequired,
  customFilters: PropTypes.arrayOf(PropTypes.object).isRequired,
  isDisabled: PropTypes.bool.isRequired,
  onFilterSelect: PropTypes.func.isRequired
};

CollectionFilterMenu.defaultProps = {
  showCustomFilters: false
};

export default CollectionFilterMenu;
