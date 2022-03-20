import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { reprocessInteractiveImportItems, updateInteractiveImportItem } from 'Store/Actions/interactiveImportActions';
import createAllMoviesSelector from 'Store/Selectors/createAllMoviesSelector';
import createDeepEqualSelector from 'Store/Selectors/createDeepEqualSelector';
import SelectMovieModalContent from './SelectMovieModalContent';

function createCleanMovieSelector() {
  return createSelector(
    createAllMoviesSelector(),
    (items) => {
      return items.map((movie) => {
        const {
          id,
          title,
          titleSlug,
          sortTitle,
          year,
          images,
          alternateTitles = []
        } = movie;

        return {
          id,
          title,
          titleSlug,
          sortTitle,
          year,
          images,
          alternateTitles,
          firstCharacter: title.charAt(0).toLowerCase()
        };
      }).sort((a, b) => {
        if (a.sortTitle < b.sortTitle) {
          return -1;
        }

        if (a.sortTitle > b.sortTitle) {
          return 1;
        }

        return 0;
      });
    }
  );
}

function createMapStateToProps() {
  return createDeepEqualSelector(
    createCleanMovieSelector(),
    (movies) => {
      return {
        items: movies
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchReprocessInteractiveImportItems: reprocessInteractiveImportItems,
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
      dispatchReprocessInteractiveImportItems,
      onModalClose
    } = this.props;

    const movie = items.find((s) => s.id === movieId);

    ids.forEach((id) => {
      dispatchUpdateInteractiveImportItem({
        id,
        movie
      });
    });

    dispatchReprocessInteractiveImportItems({ ids });

    onModalClose(true);
  };

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
  dispatchReprocessInteractiveImportItems: PropTypes.func.isRequired,
  dispatchUpdateInteractiveImportItem: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(SelectMovieModalContentConnector);
