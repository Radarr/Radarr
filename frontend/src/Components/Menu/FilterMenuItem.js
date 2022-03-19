import PropTypes from 'prop-types';
import React, { Component } from 'react';
import SelectedMenuItem from './SelectedMenuItem';

class FilterMenuItem extends Component {

  //
  // Listeners

  onPress = () => {
    const {
      filterKey,
      onPress
    } = this.props;

    onPress(filterKey);
  };

  //
  // Render

  render() {
    const {
      filterKey,
      selectedFilterKey,
      multipleSelection,
      ...otherProps
    } = this.props;

    const isSelected = multipleSelection ? selectedFilterKey.includes(filterKey) : selectedFilterKey === filterKey;

    return (
      <SelectedMenuItem
        isSelected={isSelected}
        {...otherProps}
        onPress={this.onPress}
      />
    );
  }
}

FilterMenuItem.propTypes = {
  filterKey: PropTypes.oneOfType([PropTypes.string, PropTypes.number]).isRequired,
  selectedFilterKey: PropTypes.oneOfType([PropTypes.string, PropTypes.number, PropTypes.arrayOf(PropTypes.string)]).isRequired,
  onPress: PropTypes.func.isRequired
};

export default FilterMenuItem;
