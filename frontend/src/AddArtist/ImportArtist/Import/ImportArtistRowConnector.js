import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { setImportArtistValue } from 'Store/Actions/importArtistActions';
import createAllArtistSelector from 'Store/Selectors/createAllArtistSelector';
import ImportArtistRow from './ImportArtistRow';

function createImportArtistItemSelector() {
  return createSelector(
    (state, { id }) => id,
    (state) => state.importArtist.items,
    (id, items) => {
      return _.find(items, { id }) || {};
    }
  );
}

function createMapStateToProps() {
  return createSelector(
    createImportArtistItemSelector(),
    createAllArtistSelector(),
    (item, artist) => {
      const selectedArtist = item && item.selectedArtist;
      const isExistingArtist = !!selectedArtist && _.some(artist, { foreignArtistId: selectedArtist.foreignArtistId });

      return {
        ...item,
        isExistingArtist
      };
    }
  );
}

const mapDispatchToProps = {
  setImportArtistValue
};

class ImportArtistRowConnector extends Component {

  //
  // Listeners

  onInputChange = ({ name, value }) => {
    this.props.setImportArtistValue({
      id: this.props.id,
      [name]: value
    });
  }

  //
  // Render

  render() {
    // Don't show the row until we have the information we require for it.

    const {
      items,
      monitor,
      albumFolder
    } = this.props;

    if (!items || !monitor || !albumFolder == null) {
      return null;
    }

    return (
      <ImportArtistRow
        {...this.props}
        onInputChange={this.onInputChange}
        onArtistSelect={this.onArtistSelect}
      />
    );
  }
}

ImportArtistRowConnector.propTypes = {
  rootFolderId: PropTypes.number.isRequired,
  id: PropTypes.string.isRequired,
  monitor: PropTypes.string,
  albumFolder: PropTypes.bool,
  items: PropTypes.arrayOf(PropTypes.object),
  setImportArtistValue: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(ImportArtistRowConnector);
