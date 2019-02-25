import PropTypes from 'prop-types';
import React, { Component } from 'react';
import TetherComponent from 'react-tether';
import classNames from 'classnames';
import isMobileUtil from 'Utilities/isMobile';
import { kinds, tooltipPositions } from 'Helpers/Props';
import styles from './Tooltip.css';

const baseTetherOptions = {
  skipMoveElement: true,
  constraints: [
    {
      to: 'window',
      attachment: 'together',
      pin: true
    }
  ]
};

const tetherOptions = {
  [tooltipPositions.TOP]: {
    ...baseTetherOptions,
    attachment: 'bottom center',
    targetAttachment: 'top center'
  },

  [tooltipPositions.RIGHT]: {
    ...baseTetherOptions,
    attachment: 'middle left',
    targetAttachment: 'middle right'
  },

  [tooltipPositions.BOTTOM]: {
    ...baseTetherOptions,
    attachment: 'top center',
    targetAttachment: 'bottom center'
  },

  [tooltipPositions.LEFT]: {
    ...baseTetherOptions,
    attachment: 'middle right',
    targetAttachment: 'middle left'
  }
};

class Tooltip extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isOpen: false
    };

    this._closeTimeout = null;
  }

  componentWillUnmount() {
    if (this._closeTimeout) {
      this._closeTimeout = clearTimeout(this._closeTimeout);
    }
  }

  //
  // Listeners

  onClick = () => {
    if (isMobileUtil()) {
      this.setState({ isOpen: !this.state.isOpen });
    }
  }

  onMouseEnter = () => {
    if (this._closeTimeout) {
      this._closeTimeout = clearTimeout(this._closeTimeout);
    }

    this.setState({ isOpen: true });
  }

  onMouseLeave = () => {
    this._closeTimeout = setTimeout(() => {
      this.setState({ isOpen: false });
    }, 100);
  }

  //
  // Render

  render() {
    const {
      className,
      anchor,
      tooltip,
      kind,
      position
    } = this.props;

    return (
      <TetherComponent
        classes={{
          element: styles.tether
        }}
        {...tetherOptions[position]}
      >
        <span
          className={className}
          onClick={this.onClick}
          onMouseEnter={this.onMouseEnter}
          onMouseLeave={this.onMouseLeave}
        >
          {anchor}
        </span>

        {
          this.state.isOpen &&
            <div
              className={styles.tooltipContainer}
              onMouseEnter={this.onMouseEnter}
              onMouseLeave={this.onMouseLeave}
            >
              <div
                className={classNames(
                  styles.tooltip,
                  styles[kind]
                )}
              >
                <div
                  className={classNames(
                    styles.arrow,
                    styles[kind],
                    styles[position]
                  )}
                />

                <div className={styles.body}>
                  {tooltip}
                </div>
              </div>
            </div>
        }
      </TetherComponent>
    );
  }
}

Tooltip.propTypes = {
  className: PropTypes.string,
  anchor: PropTypes.node.isRequired,
  tooltip: PropTypes.oneOfType([PropTypes.string, PropTypes.node]).isRequired,
  kind: PropTypes.oneOf([kinds.DEFAULT, kinds.INVERSE]),
  position: PropTypes.oneOf(tooltipPositions.all)
};

Tooltip.defaultProps = {
  kind: kinds.DEFAULT,
  position: tooltipPositions.TOP
};

export default Tooltip;
