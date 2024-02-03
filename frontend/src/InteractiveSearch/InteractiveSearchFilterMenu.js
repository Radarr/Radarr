import PropTypes from 'prop-types';
import React from 'react';
import FilterMenu from 'Components/Menu/FilterMenu';
import PageMenuButton from 'Components/Menu/PageMenuButton';
import { align } from 'Helpers/Props';
import InteractiveSearchFilterModalConnector from './InteractiveSearchFilterModalConnector';
import styles from './InteractiveSearch.css';

function InteractiveSearchFilterMenu(props) {
  const {
    selectedFilterKey,
    filters,
    customFilters,
    onFilterSelect
  } = props;

  return (
    <div className={styles.filterMenuContainer}>
      <FilterMenu
        alignMenu={align.RIGHT}
        selectedFilterKey={selectedFilterKey}
        filters={filters}
        customFilters={customFilters}
        buttonComponent={PageMenuButton}
        filterModalConnectorComponent={InteractiveSearchFilterModalConnector}
        onFilterSelect={onFilterSelect}
      />
    </div>
  );
}

InteractiveSearchFilterMenu.propTypes = {
  selectedFilterKey: PropTypes.oneOfType([PropTypes.string, PropTypes.number]).isRequired,
  filters: PropTypes.arrayOf(PropTypes.object).isRequired,
  customFilters: PropTypes.arrayOf(PropTypes.object).isRequired,
  onFilterSelect: PropTypes.func.isRequired
};

export default InteractiveSearchFilterMenu;
