import PropTypes from 'prop-types';
import React from 'react';
import FilterMenu from 'Components/Menu/FilterMenu';
import DiscoverMovieFilterModalConnector from 'DiscoverMovie/DiscoverMovieFilterModalConnector';
import { align } from 'Helpers/Props';

function DiscoverMovieFilterMenu(props) {
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
      filterModalConnectorComponent={DiscoverMovieFilterModalConnector}
      onFilterSelect={onFilterSelect}
    />
  );
}

DiscoverMovieFilterMenu.propTypes = {
  selectedFilterKey: PropTypes.oneOfType([PropTypes.string, PropTypes.number]).isRequired,
  filters: PropTypes.arrayOf(PropTypes.object).isRequired,
  customFilters: PropTypes.arrayOf(PropTypes.object).isRequired,
  isDisabled: PropTypes.bool.isRequired,
  onFilterSelect: PropTypes.func.isRequired
};

DiscoverMovieFilterMenu.defaultProps = {
  showCustomFilters: false
};

export default DiscoverMovieFilterMenu;
