import PropTypes from 'prop-types';
import React from 'react';
import { align } from 'Helpers/Props';
import FilterMenu from 'Components/Menu/FilterMenu';
import MenuContent from 'Components/Menu/MenuContent';
import FilterMenuItem from 'Components/Menu/FilterMenuItem';

function ArtistIndexFilterMenu(props) {
  const {
    filterKey,
    filterValue,
    isDisabled,
    onFilterSelect
  } = props;

  return (
    <FilterMenu
      isDisabled={isDisabled}
      alignMenu={align.RIGHT}
    >
      <MenuContent>
        <FilterMenuItem
          filterKey={filterKey}
          filterValue={filterValue}
          onPress={onFilterSelect}
        >
          All
        </FilterMenuItem>

        <FilterMenuItem
          name="monitored"
          value={true}
          filterKey={filterKey}
          filterValue={filterValue}
          onPress={onFilterSelect}
        >
          Monitored Only
        </FilterMenuItem>

        <FilterMenuItem
          name="status"
          value="continuing"
          filterKey={filterKey}
          filterValue={filterValue}
          onPress={onFilterSelect}
        >
          Continuing Only
        </FilterMenuItem>

        <FilterMenuItem
          name="status"
          value="ended"
          filterKey={filterKey}
          filterValue={filterValue}
          onPress={onFilterSelect}
        >
          Ended Only
        </FilterMenuItem>

        <FilterMenuItem
          name="missing"
          value={true}
          filterKey={filterKey}
          filterValue={filterValue}
          onPress={onFilterSelect}
        >
          Missing Albums
        </FilterMenuItem>
      </MenuContent>
    </FilterMenu>
  );
}

ArtistIndexFilterMenu.propTypes = {
  filterKey: PropTypes.string,
  filterValue: PropTypes.oneOfType([PropTypes.bool, PropTypes.number, PropTypes.string]),
  isDisabled: PropTypes.bool.isRequired,
  onFilterSelect: PropTypes.func.isRequired
};

export default ArtistIndexFilterMenu;
