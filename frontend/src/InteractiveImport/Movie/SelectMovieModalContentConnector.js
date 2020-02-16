import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { updateInteractiveImportItem } from 'Store/Actions/interactiveImportActions';
import createAllMoviesSelector from 'Store/Selectors/createAllMoviesSelector';
import SelectMovieModalContent from './SelectMovieModalContent';

function createMapStateToProps() {
  return createSelector(
    createAllMoviesSelector(),
    (items) => {
      return {
        items: [...items].sort((a, b) => {
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
  dispatchUpdateInteractiveImportItem: updateInteractiveImportItem
};

class SelectMovieModalContentConnector extends Component {

  //
  // Listeners

  onMovieSelect = (movieId) => {
    const {
      ids,
      items,
      dispatchUpdateInteractiveImportItem,
      onModalClose
    } = this.props;

    const movie = items.find((s) => s.id === movieId);

    ids.forEach((id) => {
      dispatchUpdateInteractiveImportItem({
        id,
        movie
      });
    });

    onModalClose(true);
  }

  //
  // Render

  render() {
    return (
      <SelectMovieModalContent
        {...this.props}
        onMovieSelect={this.onMovieSelect}
      />
    );
  }
}

SelectMovieModalContentConnector.propTypes = {
  ids: PropTypes.arrayOf(PropTypes.number).isRequired,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  dispatchUpdateInteractiveImportItem: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(SelectMovieModalContentConnector);
