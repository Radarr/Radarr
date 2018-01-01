import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { executeCommand } from 'Store/Actions/commandActions';
import * as commandNames from 'Commands/commandNames';
import AlbumSearch from './AlbumSearch';
import InteractiveAlbumSearchConnector from './InteractiveAlbumSearchConnector';

function createMapStateToProps() {
  return createSelector(
    (state) => state.releases,
    (releases) => {
      return {
        isPopulated: releases.isPopulated
      };
    }
  );
}

const mapDispatchToProps = {
  executeCommand
};

class AlbumSearchConnector extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isInteractiveSearchOpen: props.startInteractiveSearch
    };
  }

  componentDidMount() {
    if (this.props.isPopulated) {
      this.setState({ isInteractiveSearchOpen: true });
    }
  }

  //
  // Listeners

  onQuickSearchPress = () => {
    this.props.executeCommand({
      name: commandNames.ALBUM_SEARCH,
      albumIds: [this.props.albumId]
    });

    this.props.onModalClose();
  }

  onInteractiveSearchPress = () => {
    this.setState({ isInteractiveSearchOpen: true });
  }

  //
  // Render

  render() {
    if (this.state.isInteractiveSearchOpen) {
      return (
        <InteractiveAlbumSearchConnector
          {...this.props}
        />
      );
    }

    return (
      <AlbumSearch
        {...this.props}
        onQuickSearchPress={this.onQuickSearchPress}
        onInteractiveSearchPress={this.onInteractiveSearchPress}
      />
    );
  }
}

AlbumSearchConnector.propTypes = {
  albumId: PropTypes.number.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  startInteractiveSearch: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired,
  executeCommand: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(AlbumSearchConnector);
