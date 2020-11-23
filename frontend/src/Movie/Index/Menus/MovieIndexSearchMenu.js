import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Menu from 'Components/Menu/Menu';
import MenuContent from 'Components/Menu/MenuContent';
import SearchMenuItem from 'Components/Menu/SearchMenuItem';
import ToolbarMenuButton from 'Components/Menu/ToolbarMenuButton';
import { align, icons } from 'Helpers/Props';
import translate from 'Utilities/String/translate';

class MovieIndexSearchMenu extends Component {

  render() {
    const {
      isDisabled,
      onSearchPress
    } = this.props;

    return (
      <Menu
        isDisabled={isDisabled}
        alignMenu={align.RIGHT}
      >
        <ToolbarMenuButton
          iconName={icons.SEARCH}
          text="Search"
          isDisabled={isDisabled}
        />
        <MenuContent>
          <SearchMenuItem
            name="missingMoviesSearch"
            onPress={onSearchPress}
          >
            {translate('SearchMissing')}
          </SearchMenuItem>

          <SearchMenuItem
            name="cutoffUnmetMoviesSearch"
            onPress={onSearchPress}
          >
            {translate('SearchCutoffUnmet')}
          </SearchMenuItem>
        </MenuContent>
      </Menu>
    );
  }
}

MovieIndexSearchMenu.propTypes = {
  isDisabled: PropTypes.bool.isRequired,
  onSearchPress: PropTypes.func.isRequired
};

export default MovieIndexSearchMenu;
