import PropTypes from 'prop-types';
import React from 'react';
import { align } from 'Helpers/Props';
import FilterMenu from 'Components/Menu/FilterMenu';
import ArtistIndexFilterModalConnector from 'Artist/Index/ArtistIndexFilterModalConnector';

function ArtistIndexFilterMenu(props) {
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
      filterModalConnectorComponent={ArtistIndexFilterModalConnector}
      onFilterSelect={onFilterSelect}
    />
  );
}

ArtistIndexFilterMenu.propTypes = {
  selectedFilterKey: PropTypes.oneOfType([PropTypes.string, PropTypes.number]).isRequired,
  filters: PropTypes.arrayOf(PropTypes.object).isRequired,
  customFilters: PropTypes.arrayOf(PropTypes.object).isRequired,
  isDisabled: PropTypes.bool.isRequired,
  onFilterSelect: PropTypes.func.isRequired
};

ArtistIndexFilterMenu.defaultProps = {
  showCustomFilters: false
};

export default ArtistIndexFilterMenu;
