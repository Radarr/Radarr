import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { without } from 'underscore';
import { icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import FilterMenuContent from './FilterMenuContent';
import Menu from './Menu';
import ToolbarMenuButton from './ToolbarMenuButton';
import styles from './FilterMenu.css';

class FilterMenu extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isFilterModalOpen: false
    };
  }

  //
  // Listeners

  onCustomFiltersPress = () => {
    this.setState({ isFilterModalOpen: true });
  };

  onFiltersModalClose = () => {
    this.setState({ isFilterModalOpen: false });
  };

  onFilterSelect = (multipleSelection) => (key) => {
    const { onFilterSelect } = this.props;

    const newSelection = multipleSelection ? this.getNewSelections(key) : key;
    onFilterSelect(newSelection);
  }

  getNewSelections(key) {
    const { selectedFilterKey } = this.props;

    if (selectedFilterKey.find((selectedKey) => selectedKey === key)) {
      return this.processUnselection(key);
    }

    return this.processSelection(key);
  }

  processSelection(key) {
    const { filters, selectedFilterKey } = this.props;

    const selectedFilter = filters.find((filter) => filter.key === key || filter.id === key);
    const unselectedFilters = selectedFilter?.unselectFilters || [];

    return without([selectedFilter.key, ...selectedFilterKey], ...unselectedFilters);
  }

  processUnselection(key) {
    const { selectedFilterKey } = this.props;

    const newSelections = without(selectedFilterKey, key);

    return newSelections.length === 0 ? ['all'] : newSelections;
  }

  //
  // Render

  render(props) {
    const {
      className,
      isDisabled,
      selectedFilterKey,
      filters,
      customFilters,
      buttonComponent: ButtonComponent,
      filterModalConnectorComponent: FilterModalConnectorComponent,
      filterModalConnectorComponentProps,
      ...otherProps
    } = this.props;

    const showCustomFilters = !!FilterModalConnectorComponent;
    const multipleSelection = Array.isArray(selectedFilterKey);
    const indicator = multipleSelection ? !selectedFilterKey.includes('all') : selectedFilterKey !== 'all';

    return (
      <div>
        <Menu
          className={className}
          {...otherProps}
        >
          <ButtonComponent
            iconName={icons.FILTER}
            text={translate('Filter')}
            isDisabled={isDisabled}
            indicator={indicator}
          />

          <FilterMenuContent
            selectedFilterKey={selectedFilterKey}
            filters={filters}
            customFilters={customFilters}
            showCustomFilters={showCustomFilters}
            onFilterSelect={this.onFilterSelect(multipleSelection)}
            onCustomFiltersPress={this.onCustomFiltersPress}
            multipleSelection={multipleSelection}
          />

        </Menu>

        {
          showCustomFilters &&
            <FilterModalConnectorComponent
              {...filterModalConnectorComponentProps}
              isOpen={this.state.isFilterModalOpen}
              selectedFilterKey={selectedFilterKey}
              filters={filters}
              customFilters={customFilters}
              onFilterSelect={this.onFilterSelect(multipleSelection)}
              onModalClose={this.onFiltersModalClose}
            />
        }
      </div>
    );
  }
}

FilterMenu.propTypes = {
  className: PropTypes.string,
  isDisabled: PropTypes.bool.isRequired,
  selectedFilterKey: PropTypes.oneOfType([PropTypes.string, PropTypes.number, PropTypes.arrayOf(PropTypes.string)]).isRequired,
  filters: PropTypes.arrayOf(PropTypes.object).isRequired,
  customFilters: PropTypes.arrayOf(PropTypes.object).isRequired,
  buttonComponent: PropTypes.elementType.isRequired,
  filterModalConnectorComponent: PropTypes.elementType,
  filterModalConnectorComponentProps: PropTypes.object,
  onFilterSelect: PropTypes.func.isRequired
};

FilterMenu.defaultProps = {
  className: styles.filterMenu,
  isDisabled: false,
  buttonComponent: ToolbarMenuButton
};

export default FilterMenu;
