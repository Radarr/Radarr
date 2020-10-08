import classNames from 'classnames';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Icon from 'Components/Icon';
import Link from 'Components/Link/Link';
import { icons } from 'Helpers/Props';
import CheckInput from './CheckInput';
import styles from './EnhancedSelectInputOption.css';

class EnhancedSelectInputOption extends Component {

  //
  // Listeners

  onPress = () => {
    const {
      id,
      onSelect
    } = this.props;

    onSelect(id);
  }

  onCheckPress = () => {
    // CheckInput requires a handler. Swallow the change event because onPress will already handle it via event propagation.
  }

  //
  // Render

  render() {
    const {
      className,
      id,
      isSelected,
      isDisabled,
      isHidden,
      isMultiSelect,
      isMobile,
      children
    } = this.props;

    return (
      <Link
        className={classNames(
          className,
          isSelected && !isMultiSelect && styles.isSelected,
          isDisabled && !isMultiSelect && styles.isDisabled,
          isHidden && styles.isHidden,
          isMobile && styles.isMobile
        )}
        component="div"
        isDisabled={isDisabled}
        onPress={this.onPress}
      >

        {
          isMultiSelect &&
            <CheckInput
              className={styles.optionCheckInput}
              containerClassName={styles.optionCheck}
              name={`select-${id}`}
              value={isSelected}
              isDisabled={isDisabled}
              onChange={this.onCheckPress}
            />
        }

        {children}

        {
          isMobile &&
            <div className={styles.iconContainer}>
              <Icon
                name={isSelected ? icons.CHECK_CIRCLE : icons.CIRCLE_OUTLINE}
              />
            </div>
        }
      </Link>
    );
  }
}

EnhancedSelectInputOption.propTypes = {
  className: PropTypes.string.isRequired,
  id: PropTypes.oneOfType([PropTypes.string, PropTypes.number]).isRequired,
  isSelected: PropTypes.bool.isRequired,
  isDisabled: PropTypes.bool.isRequired,
  isHidden: PropTypes.bool.isRequired,
  isMultiSelect: PropTypes.bool.isRequired,
  isMobile: PropTypes.bool.isRequired,
  children: PropTypes.node.isRequired,
  onSelect: PropTypes.func.isRequired
};

EnhancedSelectInputOption.defaultProps = {
  className: styles.option,
  isDisabled: false,
  isHidden: false,
  isMultiSelect: false
};

export default EnhancedSelectInputOption;
