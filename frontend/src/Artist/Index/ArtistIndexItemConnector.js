/* eslint max-params: 0 */
import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createArtistSelector from 'Store/Selectors/createArtistSelector';
import createExecutingCommandsSelector from 'Store/Selectors/createCommandsSelector';
import createQualityProfileSelector from 'Store/Selectors/createQualityProfileSelector';
import createLanguageProfileSelector from 'Store/Selectors/createLanguageProfileSelector';
import createMetadataProfileSelector from 'Store/Selectors/createMetadataProfileSelector';
import { executeCommand } from 'Store/Actions/commandActions';
import * as commandNames from 'Commands/commandNames';

function selectShowSearchAction() {
  return createSelector(
    (state) => state.artistIndex,
    (artistIndex) => {
      const view = artistIndex.view;

      switch (view) {
        case 'posters':
          return artistIndex.posterOptions.showSearchAction;
        case 'banners':
          return artistIndex.bannerOptions.showSearchAction;
        case 'overview':
          return artistIndex.overviewOptions.showSearchAction;
        default:
          return artistIndex.tableOptions.showSearchAction;
      }
    }
  );
}

function createMapStateToProps() {
  return createSelector(
    createArtistSelector(),
    createQualityProfileSelector(),
    createLanguageProfileSelector(),
    createMetadataProfileSelector(),
    selectShowSearchAction(),
    createExecutingCommandsSelector(),
    (
      artist,
      qualityProfile,
      languageProfile,
      metadataProfile,
      showSearchAction,
      executingCommands
    ) => {
      const isRefreshingArtist = executingCommands.some((command) => {
        return (
          command.name === commandNames.REFRESH_ARTIST &&
          command.body.artistId === artist.id
        );
      });

      const isSearchingArtist = executingCommands.some((command) => {
        return (
          command.name === commandNames.ARTIST_SEARCH &&
          command.body.artistId === artist.id
        );
      });

      const latestAlbum = _.maxBy(artist.albums, (album) => album.releaseDate);

      return {
        ...artist,
        qualityProfile,
        languageProfile,
        metadataProfile,
        latestAlbum,
        showSearchAction,
        isRefreshingArtist,
        isSearchingArtist
      };
    }
  );
}

const mapDispatchToProps = {
  executeCommand
};

class ArtistIndexItemConnector extends Component {

  //
  // Listeners

  onRefreshArtistPress = () => {
    this.props.executeCommand({
      name: commandNames.REFRESH_ARTIST,
      artistId: this.props.id
    });
  }

  onSearchPress = () => {
    this.props.executeCommand({
      name: commandNames.ARTIST_SEARCH,
      artistId: this.props.id
    });
  }

  //
  // Render

  render() {
    const {
      component: ItemComponent,
      ...otherProps
    } = this.props;

    return (
      <ItemComponent
        {...otherProps}
        onRefreshArtistPress={this.onRefreshArtistPress}
        onSearchPress={this.onSearchPress}
      />
    );
  }
}

ArtistIndexItemConnector.propTypes = {
  id: PropTypes.number.isRequired,
  component: PropTypes.func.isRequired,
  executeCommand: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(ArtistIndexItemConnector);
