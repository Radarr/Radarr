import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { updateInteractiveImportItem } from 'Store/Actions/interactiveImportActions';
import createAllSeriesSelector from 'Store/Selectors/createAllSeriesSelector';
import SelectArtistModalContent from './SelectArtistModalContent';

function createMapStateToProps() {
  return createSelector(
    createAllSeriesSelector(),
    (items) => {
      return {
        items
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

  onSeriesSelect = (artistId) => {
    const series = _.find(this.props.items, { id: artistId });

    this.props.ids.forEach((id) => {
      this.props.updateInteractiveImportItem({
        id,
        series,
        seasonNumber: undefined,
        episodes: []
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
        onSeriesSelect={this.onSeriesSelect}
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
