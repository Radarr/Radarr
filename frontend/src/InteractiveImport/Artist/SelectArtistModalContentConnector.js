import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { updateInteractiveImportItem } from 'Store/Actions/interactiveImportActions';
import createAllArtistSelector from 'Store/Selectors/createAllArtistSelector';
import SelectArtistModalContent from './SelectArtistModalContent';

function createMapStateToProps() {
  return createSelector(
    createAllArtistSelector(),
    (items) => {
      return {
        items: items.sort((a, b) => {
          if (a.sortName < b.sortName) {
            return -1;
          }

          if (a.sortName > b.sortName) {
            return 1;
          }

          return 0;
        })
      };
    }
  );
}

const mapDispatchToProps = {
  updateInteractiveImportItem
};

class SelectArtistModalContentConnector extends Component {

  //
  // Listeners

  onArtistSelect = (artistId) => {
    const artist = _.find(this.props.items, { id: artistId });

    this.props.ids.forEach((id) => {
      this.props.updateInteractiveImportItem({
        id,
        artist,
        album: undefined,
        tracks: []
      });
    });

    this.props.onModalClose(true);
  }

  //
  // Render

  render() {
    return (
      <SelectArtistModalContent
        {...this.props}
        onArtistSelect={this.onArtistSelect}
      />
    );
  }
}

SelectArtistModalContentConnector.propTypes = {
  ids: PropTypes.arrayOf(PropTypes.number).isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  updateInteractiveImportItem: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(SelectArtistModalContentConnector);
