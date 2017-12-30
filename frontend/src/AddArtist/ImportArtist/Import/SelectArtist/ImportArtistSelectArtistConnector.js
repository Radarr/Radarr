import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { queueLookupArtist, setImportArtistValue } from 'Store/Actions/importArtistActions';
import createImportArtistItemSelector from 'Store/Selectors/createImportArtistItemSelector';
import ImportArtistSelectArtist from './ImportArtistSelectArtist';

function createMapStateToProps() {
  return createSelector(
    (state) => state.importArtist.isLookingUpArtist,
    createImportArtistItemSelector(),
    (isLookingUpArtist, item) => {
      return {
        isLookingUpArtist,
        ...item
      };
    }
  );
}

const mapDispatchToProps = {
  queueLookupArtist,
  setImportArtistValue
};

class ImportArtistSelectArtistConnector extends Component {

  //
  // Listeners

  onSearchInputChange = (term) => {
    this.props.queueLookupArtist({
      name: this.props.id,
      term,
      topOfQueue: true
    });
  }

  onArtistSelect = (foreignArtistId) => {
    const {
      id,
      items
    } = this.props;

    this.props.setImportArtistValue({
      id,
      selectedArtist: _.find(items, { foreignArtistId })
    });
  }

  //
  // Render

  render() {
    return (
      <ImportArtistSelectArtist
        {...this.props}
        onSearchInputChange={this.onSearchInputChange}
        onArtistSelect={this.onArtistSelect}
      />
    );
  }
}

ImportArtistSelectArtistConnector.propTypes = {
  id: PropTypes.string.isRequired,
  items: PropTypes.arrayOf(PropTypes.object),
  selectedArtist: PropTypes.object,
  isSelected: PropTypes.bool,
  queueLookupArtist: PropTypes.func.isRequired,
  setImportArtistValue: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(ImportArtistSelectArtistConnector);
