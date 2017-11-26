import PropTypes from 'prop-types';
import React from 'react';
import { icons } from 'Helpers/Props';
import Menu from 'Components/Menu/Menu';
import ToolbarMenuButton from 'Components/Menu/ToolbarMenuButton';
import styles from './FilterMenu.css';

function FilterMenu(props) {
  const {
    className,
    children,
    isDisabled,
    ...otherProps
  } = props;

  return (
    <Menu
      className={className}
      {...otherProps}
    >
      <ToolbarMenuButton
        iconName={icons.FILTER}
        text="Filter"
        isDisabled={isDisabled}
      />
      {children}
    </Menu>
  );
}

FilterMenu.propTypes = {
  className: PropTypes.string,
  children: PropTypes.node.isRequired,
  isDisabled: PropTypes.bool.isRequired
};

FilterMenu.defaultProps = {
  className: styles.filterMenu,
  isDisabled: false
};

export default FilterMenu;
