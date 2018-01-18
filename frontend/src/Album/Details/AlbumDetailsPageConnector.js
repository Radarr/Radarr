import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { push } from 'react-router-redux';
import NotFound from 'Components/NotFound';
import { fetchAlbums, clearAlbums } from 'Store/Actions/albumActions';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import AlbumDetailsConnector from './AlbumDetailsConnector';

function createMapStateToProps() {
  return createSelector(
    (state, { match }) => match,
    (state) => state.albums,
    (match, albums) => {
      const foreignAlbumId = match.params.foreignAlbumId;
      const isAlbumsFetching = albums.isFetching;
      const isAlbumsPopulated = albums.isPopulated;

      return {
        foreignAlbumId,
        isAlbumsFetching,
        isAlbumsPopulated
      };
    }
  );
}

const mapDispatchToProps = {
  push,
  fetchAlbums,
  clearAlbums
};

class AlbumDetailsPageConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.populate();
  }

  componentWillUnmount() {
    this.unpopulate();
  }

  //
  // Control

  populate = () => {
    const foreignAlbumId = this.props.foreignAlbumId;
    this.props.fetchAlbums({ foreignAlbumId });
  }

  unpopulate = () => {
    this.props.clearAlbums();
  }

  //
  // Render

  render() {
    const {
      foreignAlbumId,
      isAlbumsFetching,
      isAlbumsPopulated
    } = this.props;

    if (!foreignAlbumId) {
      return (
        <NotFound
          message="Sorry, that album cannot be found."
        />
      );
    }

    if (isAlbumsFetching) {
      return (
        <LoadingIndicator />
      );
    }

    if (!isAlbumsFetching && !isAlbumsPopulated) {
      return (
        <LoadingIndicator />
      );
    }

    if (!isAlbumsFetching && isAlbumsPopulated) {
      return (
        <AlbumDetailsConnector
          foreignAlbumId={foreignAlbumId}
        />
      );
    }
  }
}

AlbumDetailsPageConnector.propTypes = {
  foreignAlbumId: PropTypes.string,
  match: PropTypes.shape({ params: PropTypes.shape({ foreignAlbumId: PropTypes.string.isRequired }).isRequired }).isRequired,
  push: PropTypes.func.isRequired,
  fetchAlbums: PropTypes.func.isRequired,
  clearAlbums: PropTypes.func.isRequired,
  isAlbumsFetching: PropTypes.bool.isRequired,
  isAlbumsPopulated: PropTypes.bool.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(AlbumDetailsPageConnector);
