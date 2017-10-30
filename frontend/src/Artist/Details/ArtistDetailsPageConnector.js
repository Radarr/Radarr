import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { push } from 'react-router-redux';
import createAllArtistSelector from 'Store/Selectors/createAllArtistSelector';
import NotFound from 'Components/NotFound';
import ArtistDetailsConnector from './ArtistDetailsConnector';

function createMapStateToProps() {
  return createSelector(
    (state, { match }) => match,
    createAllArtistSelector(),
    (match, allArtists) => {
      const nameSlug = match.params.nameSlug;
      const artistIndex = _.findIndex(allArtists, { nameSlug });

      if (artistIndex > -1) {
        return {
          nameSlug
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
    if (!this.props.nameSlug) {
      this.props.push(`${window.Sonarr.urlBase}/`);
      return;
    }
  }

  //
  // Render

  render() {
    const {
      nameSlug
    } = this.props;

    if (!nameSlug) {
      return (
        <NotFound
          message="Sorry, that artist cannot be found."
        />
      );
    }

    return (
      <ArtistDetailsConnector
        nameSlug={nameSlug}
      />
    );
  }
}

ArtistDetailsPageConnector.propTypes = {
  nameSlug: PropTypes.string,
  match: PropTypes.shape({ params: PropTypes.shape({ nameSlug: PropTypes.string.isRequired }).isRequired }).isRequired,
  push: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(ArtistDetailsPageConnector);
