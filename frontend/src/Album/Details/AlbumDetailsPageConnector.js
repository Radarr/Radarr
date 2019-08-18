import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { push } from 'connected-react-router';
import NotFound from 'Components/NotFound';
import { fetchAlbums, clearAlbums } from 'Store/Actions/albumActions';
import PageContent from 'Components/Page/PageContent';
import PageContentBodyConnector from 'Components/Page/PageContentBodyConnector';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import AlbumDetailsConnector from './AlbumDetailsConnector';

function createMapStateToProps() {
  return createSelector(
    (state, { match }) => match,
    (state) => state.albums,
    (state) => state.artist,
    (match, albums, artist) => {
      const foreignAlbumId = match.params.foreignAlbumId;
      const isFetching = albums.isFetching || artist.isFetching;
      const isPopulated = albums.isPopulated && artist.isPopulated;

      return {
        foreignAlbumId,
        isFetching,
        isPopulated
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

  constructor(props) {
    super(props);
    this.state = { hasMounted: false };
  }
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
    this.setState({ hasMounted: true });
    this.props.fetchAlbums({
      foreignAlbumId,
      includeAllArtistAlbums: true
    });
  }

  unpopulate = () => {
    this.props.clearAlbums();
  }

  //
  // Render

  render() {
    const {
      foreignAlbumId,
      isFetching,
      isPopulated
    } = this.props;

    if (!foreignAlbumId) {
      return (
        <NotFound
          message="Sorry, that album cannot be found."
        />
      );
    }

    if ((isFetching || !this.state.hasMounted) ||
        (!isFetching && !isPopulated)) {
      return (
        <PageContent title='loading'>
          <PageContentBodyConnector>
            <LoadingIndicator />
          </PageContentBodyConnector>
        </PageContent>
      );
    }

    if (!isFetching && isPopulated && this.state.hasMounted) {
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
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(AlbumDetailsPageConnector);
