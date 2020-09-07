import PropTypes from 'prop-types';
import React from 'react';
import AuthorIndexFilterModalConnector from 'Author/Index/AuthorIndexFilterModalConnector';
import FilterMenu from 'Components/Menu/FilterMenu';
import { align } from 'Helpers/Props';

function AuthorIndexFilterMenu(props) {
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
      filterModalConnectorComponent={AuthorIndexFilterModalConnector}
      onFilterSelect={onFilterSelect}
    />
  );
}

AuthorIndexFilterMenu.propTypes = {
  selectedFilterKey: PropTypes.oneOfType([PropTypes.string, PropTypes.number]).isRequired,
  filters: PropTypes.arrayOf(PropTypes.object).isRequired,
  customFilters: PropTypes.arrayOf(PropTypes.object).isRequired,
  isDisabled: PropTypes.bool.isRequired,
  onFilterSelect: PropTypes.func.isRequired
};

AuthorIndexFilterMenu.defaultProps = {
  showCustomFilters: false
};

export default AuthorIndexFilterMenu;
