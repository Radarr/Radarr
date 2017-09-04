import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import LazyLoad from 'react-lazyload';

const posterPlaceholder = 'data:image/png;base64,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';

function findPoster(images) {
  return _.find(images, { coverType: 'poster' });
}

function getPosterUrl(poster, size) {
  if (poster) {
    // Remove protocol
    let url = poster.url.replace(/^https?:/, '');
    url = url.replace('poster.jpg', `poster-${size}.jpg`);

    return url;
  }
}

class ArtistPoster extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    const pixelRatio = Math.floor(window.devicePixelRatio);

    const {
      images,
      size
    } = props;

    const poster = findPoster(images);

    this.state = {
      pixelRatio,
      poster,
      posterUrl: getPosterUrl(poster, pixelRatio * size),
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

    const poster = findPoster(images);

    if (poster && poster.url !== this.state.poster.url) {
      this.setState({
        poster,
        posterUrl: getPosterUrl(poster, pixelRatio * size),
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
      posterUrl,
      hasError,
      isLoaded
    } = this.state;

    if (hasError || !posterUrl) {
      return (
        <img
          className={className}
          style={style}
          src={posterPlaceholder}
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
              src={posterPlaceholder}
            />
          }
        >
          <img
            className={className}
            style={style}
            src={posterUrl}
            onError={this.onError}
          />
        </LazyLoad>
      );
    }

    return (
      <img
        className={className}
        style={style}
        src={isLoaded ? posterUrl : posterPlaceholder}
        onError={this.onError}
        onLoad={this.onLoad}
      />
    );
  }
}

ArtistPoster.propTypes = {
  className: PropTypes.string,
  style: PropTypes.object,
  images: PropTypes.arrayOf(PropTypes.object).isRequired,
  size: PropTypes.number.isRequired,
  lazy: PropTypes.bool.isRequired,
  overflow: PropTypes.bool.isRequired
};

ArtistPoster.defaultProps = {
  size: 250,
  lazy: true,
  overflow: false
};

export default ArtistPoster;
