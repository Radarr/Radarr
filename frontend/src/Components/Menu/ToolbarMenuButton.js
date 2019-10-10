import PropTypes from 'prop-types';
import React from 'react';
import classNames from 'classnames';
import { icons } from 'Helpers/Props';
import Icon from 'Components/Icon';
import MenuButton from 'Components/Menu/MenuButton';
import styles from './ToolbarMenuButton.css';

function ToolbarMenuButton(props) {
  const {
    iconName,
    indicator,
    text,
    ...otherProps
  } = props;

  return (
    <MenuButton
      className={styles.menuButton}
      {...otherProps}
    >
      <div>
        <Icon
          name={iconName}
          size={21}
        />

        {
          indicator &&
            <span
              className={classNames(
                styles.indicatorContainer,
                'fa-layers fa-fw'
              )}
            >
              <Icon
                className={styles.indicatorBackground}
                name={icons.CIRCLE}
                size={10}
              />
            </span>
        }

        <div className={styles.labelContainer}>
          <div className={styles.label}>
            {text}
          </div>
        </div>
      </div>
    </MenuButton>
  );
}

ToolbarMenuButton.propTypes = {
  iconName: PropTypes.object.isRequired,
  text: PropTypes.string,
  indicator: PropTypes.bool.isRequired
};

ToolbarMenuButton.defaultProps = {
  indicator: false
};

export default ToolbarMenuButton;
