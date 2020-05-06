import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createAllArtistSelector from 'Store/Selectors/createAllArtistSelector';
import { executeCommand } from 'Store/Actions/commandActions';
import * as commandNames from 'Commands/commandNames';
import RetagArtistModalContent from './RetagArtistModalContent';

function createMapStateToProps() {
  return createSelector(
    (state, { authorIds }) => authorIds,
    createAllArtistSelector(),
    (authorIds, allArtists) => {
      const artist = _.intersectionWith(allArtists, authorIds, (s, id) => {
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

class RetagArtistModalContentConnector extends Component {

  //
  // Listeners

  onRetagArtistPress = () => {
    this.props.executeCommand({
      name: commandNames.RETAG_ARTIST,
      authorIds: this.props.authorIds
    });

    this.props.onModalClose(true);
  }

  //
  // Render

  render(props) {
    return (
      <RetagArtistModalContent
        {...this.props}
        onRetagArtistPress={this.onRetagArtistPress}
      />
    );
  }
}

RetagArtistModalContentConnector.propTypes = {
  authorIds: PropTypes.arrayOf(PropTypes.number).isRequired,
  onModalClose: PropTypes.func.isRequired,
  executeCommand: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(RetagArtistModalContentConnector);
