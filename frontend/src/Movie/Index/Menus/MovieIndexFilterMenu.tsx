import React from 'react';
import { CustomFilter } from 'App/State/AppState';
import FilterMenu from 'Components/Menu/FilterMenu';
import { align } from 'Helpers/Props';
import MovieIndexFilterModal from 'Movie/Index/MovieIndexFilterModal';

interface MovieIndexFilterMenuProps {
  selectedFilterKey: string | number;
  filters: object[];
  customFilters: CustomFilter[];
  isDisabled: boolean;
  onFilterSelect(filterName: string): unknown;
}

function MovieIndexFilterMenu(props: MovieIndexFilterMenuProps) {
  const {
    selectedFilterKey,
    filters,
    customFilters,
    isDisabled,
    onFilterSelect,
  } = props;

  return (
    <FilterMenu
      alignMenu={align.RIGHT}
      isDisabled={isDisabled}
      selectedFilterKey={selectedFilterKey}
      filters={filters}
      customFilters={customFilters}
      filterModalConnectorComponent={MovieIndexFilterModal}
      onFilterSelect={onFilterSelect}
    />
  );
}

MovieIndexFilterMenu.defaultProps = {
  showCustomFilters: false,
};

export default MovieIndexFilterMenu;
