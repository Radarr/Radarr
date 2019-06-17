import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Scroller from 'Components/Scroller/Scroller';
import styles from './MenuContent.css';

class MenuContent extends Component {

  //
  // Render

  render() {
    const {
      forwardedRef,
      className,
      children,
      style,
      isOpen
    } = this.props;

    return (
      <div
        ref={forwardedRef}
        className={className}
        style={style}
      >
        {
          isOpen ?
            <Scroller className={styles.scroller}>
              {children}
            </Scroller> :
            null
        }
      </div>
    );
  }
}

MenuContent.propTypes = {
  forwardedRef: PropTypes.func,
  className: PropTypes.string,
  children: PropTypes.node.isRequired,
  style: PropTypes.object,
  isOpen: PropTypes.bool
};

MenuContent.defaultProps = {
  className: styles.menuContent
};

export default MenuContent;
