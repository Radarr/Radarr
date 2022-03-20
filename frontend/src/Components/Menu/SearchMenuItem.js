import PropTypes from 'prop-types';
import React, { Component } from 'react';
import MenuItem from './MenuItem';

class SearchMenuItem extends Component {

  //
  // Listeners

  onPress = () => {
    const {
      name,
      onPress
    } = this.props;

    onPress(name);
  };

  //
  // Render

  render() {
    const {
      children,
      ...otherProps
    } = this.props;

    return (
      <MenuItem
        {...otherProps}
        onPress={this.onPress}
      >
        <div>
          {children}
        </div>
      </MenuItem>
    );
  }
}

SearchMenuItem.propTypes = {
  name: PropTypes.string,
  children: PropTypes.node.isRequired,
  onPress: PropTypes.func.isRequired
};

export default SearchMenuItem;
