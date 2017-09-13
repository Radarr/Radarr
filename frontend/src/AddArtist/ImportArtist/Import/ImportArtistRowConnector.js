import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { queueLookupSeries, setImportArtistValue } from 'Store/Actions/importArtistActions';
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
    (item, series) => {
      const selectedSeries = item && item.selectedSeries;
      const isExistingArtist = !!selectedSeries && _.some(series, { foreignArtistId: selectedSeries.foreignArtistId });

      return {
        ...item,
        isExistingArtist
      };
    }
  );
}

const mapDispatchToProps = {
  queueLookupSeries,
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
      // seriesType,
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
  // seriesType: PropTypes.string,
  albumFolder: PropTypes.bool,
  items: PropTypes.arrayOf(PropTypes.object),
  queueLookupSeries: PropTypes.func.isRequired,
  setImportArtistValue: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(ImportArtistRowConnector);
