import PropTypes from 'prop-types';
import React, { Component } from 'react';
import translate from 'Utilities/String/translate';
import FilterMenuItem from './FilterMenuItem';
import MenuContent from './MenuContent';
import MenuItem from './MenuItem';
import MenuItemSeparator from './MenuItemSeparator';

class FilterMenuContent extends Component {

  //
  // Render

  render() {
    const {
      selectedFilterKey,
      filters,
      customFilters,
      showCustomFilters,
      onFilterSelect,
      onCustomFiltersPress,
      multipleSelection,
      ...otherProps
    } = this.props;

    return (
      <MenuContent {...otherProps}>
        {
          filters.map((filter) => {
            return (
              <FilterMenuItem
                key={filter.key}
                filterKey={filter.key}
                selectedFilterKey={selectedFilterKey}
                onPress={onFilterSelect}
                multipleSelection={multipleSelection}
              >
                {filter.label}
              </FilterMenuItem>
            );
          })
        }

        {
          customFilters.map((filter) => {
            return (
              <FilterMenuItem
                key={filter.id}
                filterKey={filter.id}
                selectedFilterKey={selectedFilterKey}
                onPress={onFilterSelect}
                multipleSelection={multipleSelection}
              >
                {filter.label}
              </FilterMenuItem>
            );
          })
        }

        {
          showCustomFilters &&
            <MenuItemSeparator />
        }

        {
          showCustomFilters &&
            <MenuItem onPress={onCustomFiltersPress}>
              {translate('CustomFilters')}
            </MenuItem>
        }
      </MenuContent>
    );
  }
}

FilterMenuContent.propTypes = {
  selectedFilterKey: PropTypes.oneOfType([PropTypes.string, PropTypes.number, PropTypes.arrayOf(PropTypes.string)]).isRequired,
  filters: PropTypes.arrayOf(PropTypes.object).isRequired,
  customFilters: PropTypes.arrayOf(PropTypes.object).isRequired,
  showCustomFilters: PropTypes.bool.isRequired,
  onFilterSelect: PropTypes.func.isRequired,
  onCustomFiltersPress: PropTypes.func.isRequired,
  multipleSelection: PropTypes.bool.isRequired
};

FilterMenuContent.defaultProps = {
  showCustomFilters: false
};

export default FilterMenuContent;
