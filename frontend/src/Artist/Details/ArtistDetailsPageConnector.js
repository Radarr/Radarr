import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { push } from 'connected-react-router';
import createAllArtistSelector from 'Store/Selectors/createAllArtistSelector';
import NotFound from 'Components/NotFound';
import ArtistDetailsConnector from './ArtistDetailsConnector';

function createMapStateToProps() {
  return createSelector(
    (state, { match }) => match,
    createAllArtistSelector(),
    (match, allArtists) => {
      const foreignArtistId = match.params.foreignArtistId;
      const artistIndex = _.findIndex(allArtists, { foreignArtistId });

      if (artistIndex > -1) {
        return {
          foreignArtistId
        };
      }

      return {};
    }
  );
}

const mapDispatchToProps = {
  push
};

class ArtistDetailsPageConnector extends Component {

  //
  // Lifecycle

  componentDidUpdate(prevProps) {
    if (!this.props.foreignArtistId) {
      this.props.push(`${window.Lidarr.urlBase}/`);
      return;
    }
  }

  //
  // Render

  render() {
    const {
      foreignArtistId
    } = this.props;

    if (!foreignArtistId) {
      return (
        <NotFound
          message="Sorry, that artist cannot be found."
        />
      );
    }

    return (
      <ArtistDetailsConnector
        foreignArtistId={foreignArtistId}
      />
    );
  }
}

ArtistDetailsPageConnector.propTypes = {
  foreignArtistId: PropTypes.string,
  match: PropTypes.shape({ params: PropTypes.shape({ foreignArtistId: PropTypes.string.isRequired }).isRequired }).isRequired,
  push: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(ArtistDetailsPageConnector);
