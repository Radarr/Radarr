import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import LazyLoad from 'react-lazyload';

const logoPlaceholder = 'data:image/png;base64,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';

function findLogo(images) {
  return _.find(images, { coverType: 'logo' });
}

function getLogoUrl(logo, size) {
  if (logo) {
    // Remove protocol
    let url = logo.url.replace(/^https?:/, '');
    url = url.replace('logo.jpg', `logo-${size}.jpg`);

    return url;
  }
}

class ArtistLogo extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    const pixelRatio = Math.floor(window.devicePixelRatio);

    const {
      images,
      size
    } = props;

    const logo = findLogo(images);

    this.state = {
      pixelRatio,
      logo,
      logoUrl: getLogoUrl(logo, pixelRatio * size),
      hasError: false,
      isLoaded: false
    };
  }

  componentDidUpdate(prevProps) {
    const {
      images,
      size
    } = this.props;

    const {
      pixelRatio
    } = this.state;

    const logo = findLogo(images);

    if (logo && logo.url !== this.state.logo.url) {
      this.setState({
        logo,
        logoUrl: getLogoUrl(logo, pixelRatio * size),
        hasError: false,
        isLoaded: false
      });
    }
  }

  //
  // Listeners

  onError = () => {
    this.setState({ hasError: true });
  }

  onLoad = () => {
    this.setState({ isLoaded: true });
  }

  //
  // Render

  render() {
    const {
      className,
      style,
      size,
      lazy,
      overflow
    } = this.props;

    const {
      logoUrl,
      hasError,
      isLoaded
    } = this.state;

    if (hasError || !logoUrl) {
      return (
        <img
          className={className}
          style={style}
          src={logoPlaceholder}
        />
      );
    }

    if (lazy) {
      return (
        <LazyLoad
          height={size}
          offset={100}
          overflow={overflow}
          placeholder={
            <img
              className={className}
              style={style}
              src={logoPlaceholder}
            />
          }
        >
          <img
            className={className}
            style={style}
            src={logoUrl}
            onError={this.onError}
          />
        </LazyLoad>
      );
    }

    return (
      <img
        className={className}
        style={style}
        src={isLoaded ? logoUrl : logoPlaceholder}
        onError={this.onError}
        onLoad={this.onLoad}
      />
    );
  }
}

ArtistLogo.propTypes = {
  className: PropTypes.string,
  style: PropTypes.object,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  size: PropTypes.number.isRequired,
  lazy: PropTypes.bool.isRequired,
  overflow: PropTypes.bool.isRequired
};

ArtistLogo.defaultProps = {
  size: 250,
  lazy: true,
  overflow: false
};

export default ArtistLogo;
