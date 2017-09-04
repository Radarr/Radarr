import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { createSelector } from 'reselect';
import connectSection from 'Store/connectSection';
import { fetchEpisodes, setEpisodesSort, clearEpisodes } from 'Store/Actions/episodeActions';
import { updateInteractiveImportItem } from 'Store/Actions/interactiveImportActions';
import createClientSideCollectionSelector from 'Store/Selectors/createClientSideCollectionSelector';
import SelectEpisodeModalContent from './SelectEpisodeModalContent';

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

class SelectEpisodeModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      artistId,
      seasonNumber
    } = this.props;

    this.props.fetchEpisodes({ artistId, seasonNumber });
  }

  componentWillUnmount() {
    // This clears the episodes for the queue and hides the queue
    // We'll need another place to store episodes for manual import
    this.props.clearEpisodes();
  }

  //
  // Listeners

  onSortPress = (sortKey, sortDirection) => {
    this.props.setEpisodesSort({ sortKey, sortDirection });
  }

  onEpisodesSelect = (episodeIds) => {
    const episodes = _.reduce(this.props.items, (acc, item) => {
      if (episodeIds.indexOf(item.id) > -1) {
        acc.push(item);
      }

      return acc;
    }, []);

    this.props.updateInteractiveImportItem({
      id: this.props.id,
      episodes: _.sortBy(episodes, 'episodeNumber')
    });

    this.props.onModalClose(true);
  }

  //
  // Render

  render() {
    return (
      <SelectEpisodeModalContent
        {...this.props}
        onSortPress={this.onSortPress}
        onEpisodesSelect={this.onEpisodesSelect}
      />
    );
  }
}

SelectEpisodeModalContentConnector.propTypes = {
  id: PropTypes.number.isRequired,
  artistId: PropTypes.number.isRequired,
  seasonNumber: PropTypes.number.isRequired,
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
               )(SelectEpisodeModalContentConnector);
