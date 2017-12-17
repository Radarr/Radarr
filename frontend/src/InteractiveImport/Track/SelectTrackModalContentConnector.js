import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { createSelector } from 'reselect';
import connectSection from 'Store/connectSection';
import { fetchTracks, setTracksSort, clearTracks } from 'Store/Actions/trackActions';
import { updateInteractiveImportItem } from 'Store/Actions/interactiveImportActions';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import SelectTrackModalContent from './SelectTrackModalContent';

function createMapStateToProps() {
  return createSelector(
    createClientSideCollectionSelector(),
    (tracks) => {
      return tracks;
    }
  );
}

const mapDispatchToProps = {
  fetchTracks,
  setTracksSort,
  clearTracks,
  updateInteractiveImportItem
};

class SelectTrackModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      artistId,
      albumId
    } = this.props;

    this.props.fetchTracks({ artistId, albumId });
  }

  componentWillUnmount() {
    // This clears the tracks for the queue and hides the queue
    // We'll need another place to store tracks for manual import
    this.props.clearTracks();
  }

  //
  // Listeners

  onSortPress = (sortKey, sortDirection) => {
    this.props.setTracksSort({ sortKey, sortDirection });
  }

  onTracksSelect = (trackIds) => {
    const tracks = _.reduce(this.props.items, (acc, item) => {
      if (trackIds.indexOf(item.id) > -1) {
        acc.push(item);
      }

      return acc;
    }, []);

    this.props.updateInteractiveImportItem({
      id: this.props.id,
      tracks: _.sortBy(tracks, 'trackNumber')
    });

    this.props.onModalClose(true);
  }

  //
  // Render

  render() {
    return (
      <SelectTrackModalContent
        {...this.props}
        onSortPress={this.onSortPress}
        onTracksSelect={this.onTracksSelect}
      />
    );
  }
}

SelectTrackModalContentConnector.propTypes = {
  id: PropTypes.number.isRequired,
  artistId: PropTypes.number.isRequired,
  albumId: PropTypes.number.isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  fetchTracks: PropTypes.func.isRequired,
  setTracksSort: PropTypes.func.isRequired,
  clearTracks: PropTypes.func.isRequired,
  updateInteractiveImportItem: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connectSection(
  createMapStateToProps,
  mapDispatchToProps,
  undefined,
  undefined,
  { section: 'tracks' }
)(SelectTrackModalContentConnector);
