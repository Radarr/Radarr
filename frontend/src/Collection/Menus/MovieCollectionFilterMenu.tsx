import React from 'react';
import { CustomFilter, Filter } from 'App/State/AppState';
import MovieCollectionFilterModal from 'Collection/MovieCollectionFilterModal';
import FilterMenu from 'Components/Menu/FilterMenu';

interface MovieCollectionFilterMenuProps {
  selectedFilterKey: string | number;
  filters: Filter[];
  customFilters: CustomFilter[];
  isDisabled: boolean;
  onFilterSelect: (filter: number | string) => void;
}

function MovieCollectionFilterMenu({
  selectedFilterKey,
  filters,
  customFilters,
  isDisabled,
  onFilterSelect,
}: MovieCollectionFilterMenuProps) {
  return (
    <FilterMenu
      alignMenu="right"
      isDisabled={isDisabled}
      selectedFilterKey={selectedFilterKey}
      filters={filters}
      customFilters={customFilters}
      filterModalConnectorComponent={MovieCollectionFilterModal}
      onFilterSelect={onFilterSelect}
    />
  );
}

export default MovieCollectionFilterMenu;
