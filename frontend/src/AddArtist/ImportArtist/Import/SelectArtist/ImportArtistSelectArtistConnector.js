import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { queueLookupSeries, setImportArtistValue } from 'Store/Actions/importArtistActions';
import createImportArtistItemSelector from 'Store/Selectors/createImportArtistItemSelector';
import ImportArtistSelectArtist from './ImportArtistSelectArtist';

function createMapStateToProps() {
  return createSelector(
    createImportArtistItemSelector(),
    (item) => {
      return item;
    }
  );
}

const mapDispatchToProps = {
  queueLookupSeries,
  setImportArtistValue
};

class ImportArtistSelectArtistConnector extends Component {

  //
  // Listeners

  onSearchInputChange = (term) => {
    this.props.queueLookupSeries({
      name: this.props.id,
      term
    });
  }

  onArtistSelect = (foreignArtistId) => {
    const {
      id,
      items
    } = this.props;

    this.props.setImportArtistValue({
      id,
      selectedSeries: _.find(items, { foreignArtistId })
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
  selectedSeries: PropTypes.object,
  isSelected: PropTypes.bool,
  queueLookupSeries: PropTypes.func.isRequired,
  setImportArtistValue: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(ImportArtistSelectArtistConnector);
