import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { updateInteractiveImportItem } from 'Store/Actions/interactiveImportActions';
import createAllMoviesSelector from 'Store/Selectors/createAllMoviesSelector';
import SelectSeriesModalContent from './SelectSeriesModalContent';

function createMapStateToProps() {
  return createSelector(
    createAllMoviesSelector(),
    (items) => {
      return {
        items: items.sort((a, b) => {
          if (a.sortTitle < b.sortTitle) {
            return -1;
          }

          if (a.sortTitle > b.sortTitle) {
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

class SelectSeriesModalContentConnector extends Component {

  //
  // Listeners

  onMovieSelect = (seriesId) => {
    const series = _.find(this.props.items, { id: seriesId });

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
      <SelectSeriesModalContent
        {...this.props}
        onMovieSelect={this.onMovieSelect}
      />
    );
  }
}

SelectSeriesModalContentConnector.propTypes = {
  ids: PropTypes.arrayOf(PropTypes.number).isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  updateInteractiveImportItem: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(SelectSeriesModalContentConnector);
