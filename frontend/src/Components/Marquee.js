import PropTypes from 'prop-types';
import React, { Component } from 'react';

const FPS = 20;
const STEP = 1;
const TIMEOUT = 1 / FPS * 1000;

class Marquee extends Component {

  static propTypes = {
    text: PropTypes.string,
    title: PropTypes.string,
    hoverToStop: PropTypes.bool,
    loop: PropTypes.bool,
    className: PropTypes.string
  };

  static defaultProps = {
    text: '',
    title: '',
    hoverToStop: true,
    loop: false
  };

  state = {
    animatedWidth: 0,
    overflowWidth: 0,
    direction: 0
  };

  componentDidMount() {
    this.measureText();

    if (this.props.hoverToStop) {
      this.startAnimation();
    }
  }

  componentWillReceiveProps(nextProps) {
    if (this.props.text.length !== nextProps.text.length) {
      clearTimeout(this.marqueeTimer);
      this.setState({ animatedWidth: 0, direction: 0 });
    }
  }

  componentDidUpdate() {
    this.measureText();

    if (this.props.hoverToStop) {
      this.startAnimation();
    }
  }

  componentWillUnmount() {
    clearTimeout(this.marqueeTimer);
  }

  onHandleMouseEnter = () => {
    if (this.props.hoverToStop) {
      clearTimeout(this.marqueeTimer);
    } else if (this.state.overflowWidth > 0) {
      this.startAnimation();
    }
  };

  onHandleMouseLeave = () => {
    if (this.props.hoverToStop && this.state.overflowWidth > 0) {
      this.startAnimation();
    } else {
      clearTimeout(this.marqueeTimer);
      this.setState({ animatedWidth: 0 });
    }
  };

  startAnimation = () => {
    clearTimeout(this.marqueeTimer);
    const isLeading = this.state.animatedWidth === 0;
    const timeout = isLeading ? 0 : TIMEOUT;

    const animate = () => {
      const { overflowWidth } = this.state;
      let animatedWidth = this.state.animatedWidth;
      let direction = this.state.direction;

      if (direction === 0) {
        animatedWidth = this.state.animatedWidth + STEP;
      } else {
        animatedWidth = this.state.animatedWidth - STEP;
      }

      const isRoundOver = animatedWidth < 0;
      const endOfText = animatedWidth > overflowWidth;

      if (endOfText) {
        direction = direction === 1;
      }

      if (isRoundOver) {
        if (this.props.loop) {
          direction = direction === 0;
        } else {
          return;
        }
      }

      this.setState({ animatedWidth, direction });
      this.marqueeTimer = setTimeout(animate, TIMEOUT);
    };

    this.marqueeTimer = setTimeout(animate, timeout);
  };

  measureText = () => {
    const container = this.container;
    const node = this.text;

    if (container && node) {
      const containerWidth = container.offsetWidth;
      const textWidth = node.offsetWidth;
      const overflowWidth = textWidth - containerWidth;

      if (overflowWidth !== this.state.overflowWidth) {
        this.setState({ overflowWidth });
      }
    }
  };

  render() {
    const style = {
      position: 'relative',
      right: this.state.animatedWidth,
      whiteSpace: 'nowrap'
    };

    if (this.state.overflowWidth < 0) {
      return (
        <div
          ref={(el) => {
            this.container = el;
          }}
          className={`ui-marquee ${this.props.className}`}
          style={{ overflow: 'hidden' }}
        >
          <span
            ref={(el) => {
              this.text = el;
            }}
            style={style}
            title={(this.props.title && (this.props.text !== this.props.title)) ? `Original Title: ${this.props.title}` : this.props.text}
          >
            {this.props.text}
          </span>
        </div>
      );
    }

    return (
      <div
        ref={(el) => {
          this.container = el;
        }}
        className={`ui-marquee ${this.props.className}`.trim()}
        style={{ overflow: 'hidden' }}
        onMouseEnter={this.onHandleMouseEnter}
        onMouseLeave={this.onHandleMouseLeave}
      >
        <span
          ref={(el) => {
            this.text = el;
          }}
          style={style}
          title={(this.props.title && (this.props.text !== this.props.title)) ? `Original Title: ${this.props.title}` : this.props.text}
        >
          {this.props.text}
        </span>
      </div>
    );
  }
}

export default Marquee;
