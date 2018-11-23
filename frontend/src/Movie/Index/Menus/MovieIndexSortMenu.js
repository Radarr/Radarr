import PropTypes from 'prop-types';
import React from 'react';
import { align, sortDirections } from 'Helpers/Props';
import SortMenu from 'Components/Menu/SortMenu';
import MenuContent from 'Components/Menu/MenuContent';
import SortMenuItem from 'Components/Menu/SortMenuItem';

function MovieIndexSortMenu(props) {
  const {
    sortKey,
    sortDirection,
    isDisabled,
    onSortSelect
  } = props;

  return (
    <SortMenu
      isDisabled={isDisabled}
      alignMenu={align.RIGHT}
    >
      <MenuContent>
        <SortMenuItem
          name="status"
          sortKey={sortKey}
          sortDirection={sortDirection}
          onPress={onSortSelect}
        >
          Monitored/Status
        </SortMenuItem>

        <SortMenuItem
          name="sortTitle"
          sortKey={sortKey}
          sortDirection={sortDirection}
          onPress={onSortSelect}
        >
          Title
        </SortMenuItem>

        <SortMenuItem
          name="studio"
          sortKey={sortKey}
          sortDirection={sortDirection}
          onPress={onSortSelect}
        >
          Studio
        </SortMenuItem>

        <SortMenuItem
          name="qualityProfileId"
          sortKey={sortKey}
          sortDirection={sortDirection}
          onPress={onSortSelect}
        >
          Quality Profile
        </SortMenuItem>

        <SortMenuItem
          name="added"
          sortKey={sortKey}
          sortDirection={sortDirection}
          onPress={onSortSelect}
        >
          Added
        </SortMenuItem>

        <SortMenuItem
          name="inCinemas"
          sortKey={sortKey}
          sortDirection={sortDirection}
          onPress={onSortSelect}
        >
          In Cinemas
        </SortMenuItem>

        <SortMenuItem
          name="physicalRelease"
          sortKey={sortKey}
          sortDirection={sortDirection}
          onPress={onSortSelect}
        >
          Physical Release
        </SortMenuItem>

        <SortMenuItem
          name="path"
          sortKey={sortKey}
          sortDirection={sortDirection}
          onPress={onSortSelect}
        >
          Path
        </SortMenuItem>
      </MenuContent>
    </SortMenu>
  );
}

MovieIndexSortMenu.propTypes = {
  sortKey: PropTypes.string,
  sortDirection: PropTypes.oneOf(sortDirections.all),
  isDisabled: PropTypes.bool.isRequired,
  onSortSelect: PropTypes.func.isRequired
};

export default MovieIndexSortMenu;
