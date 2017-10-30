import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createAllArtistSelector from 'Store/Selectors/createAllArtistSelector';
import { executeCommand } from 'Store/Actions/commandActions';
import * as commandNames from 'Commands/commandNames';
import OrganizeArtistModalContent from './OrganizeArtistModalContent';

function createMapStateToProps() {
  return createSelector(
    (state, { artistIds }) => artistIds,
    createAllArtistSelector(),
    (artistIds, allArtists) => {
      const artist = _.intersectionWith(allArtists, artistIds, (s, id) => {
        return s.id === id;
      });

      const sortedArtist = _.orderBy(artist, 'sortName');
      const artistNames = _.map(sortedArtist, 'artistName');

      return {
        artistNames
      };
    }
  );
}

const mapDispatchToProps = {
  executeCommand
};

class OrganizeArtistModalContentConnector extends Component {

  //
  // Listeners

  onOrganizeArtistPress = () => {
    this.props.executeCommand({
      name: commandNames.RENAME_ARTIST,
      artistIds: this.props.artistIds
    });

    this.props.onModalClose(true);
  }

  //
  // Render

  render(props) {
    return (
      <OrganizeArtistModalContent
        {...this.props}
        onOrganizeArtistPress={this.onOrganizeArtistPress}
      />
    );
  }
}

OrganizeArtistModalContentConnector.propTypes = {
  artistIds: PropTypes.arrayOf(PropTypes.number).isRequired,
  onModalClose: PropTypes.func.isRequired,
  executeCommand: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(OrganizeArtistModalContentConnector);
