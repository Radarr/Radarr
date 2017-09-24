import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import connectSection from 'Store/connectSection';
import { createSelector } from 'reselect';
import { updateInteractiveImportItem } from 'Store/Actions/interactiveImportActions';
import { fetchEpisodes, setEpisodesSort, clearEpisodes } from 'Store/Actions/episodeActions';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import SelectAlbumModalContent from './SelectAlbumModalContent';

function createMapStateToProps() {
  return createSelector(
    createClientSideCollectionSelector(),
    (episodes) => {
      return episodes;
    }
  );
}

const mapDispatchToProps = {
  fetchEpisodes,
  setEpisodesSort,
  clearEpisodes,
  updateInteractiveImportItem
};

class SelectAlbumModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      artistId
    } = this.props;

    this.props.fetchEpisodes({ artistId });
  }

  componentWillUnmount() {
    // This clears the albums for the queue and hides the queue
    // We'll need another place to store albums for manual import
    this.props.clearEpisodes();
  }

  //
  // Listeners

  onAlbumSelect = (albumId) => {
    const album = _.find(this.props.items, { id: albumId });

    this.props.ids.forEach((id) => {
      this.props.updateInteractiveImportItem({
        id,
        album,
        episodes: []
      });
    });

    this.props.onModalClose(true);
  }

  //
  // Render

  render() {
    return (
      <SelectAlbumModalContent
        {...this.props}
        onAlbumSelect={this.onAlbumSelect}
      />
    );
  }
}

SelectAlbumModalContentConnector.propTypes = {
  ids: PropTypes.arrayOf(PropTypes.number).isRequired,
  artistId: PropTypes.number.isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  fetchEpisodes: PropTypes.func.isRequired,
  setEpisodesSort: PropTypes.func.isRequired,
  clearEpisodes: PropTypes.func.isRequired,
  updateInteractiveImportItem: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connectSection(
                createMapStateToProps,
                mapDispatchToProps,
                undefined,
                undefined,
                { section: 'episodes' }
              )(SelectAlbumModalContentConnector);
