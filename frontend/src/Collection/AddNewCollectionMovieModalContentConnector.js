import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { addMovie, setMovieCollectionValue } from 'Store/Actions/movieCollectionActions';
import createCollectionSelector from 'Store/Selectors/createCollectionSelector';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import createSystemStatusSelector from 'Store/Selectors/createSystemStatusSelector';
import selectSettings from 'Store/Selectors/selectSettings';
import AddNewMovieModalContent from './AddNewCollectionMovieModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.movieCollections,
    createCollectionSelector(),
    createDimensionsSelector(),
    createSystemStatusSelector(),
    (discoverMovieState, collection, dimensions, systemStatus) => {
      const {
        isAdding,
        addError,
        pendingChanges
      } = discoverMovieState;

      const collectionDefaults = {
        rootFolderPath: collection.rootFolderPath,
        monitor: 'movieOnly',
        qualityProfileIds: collection.qualityProfileIds,
        minimumAvailability: collection.minimumAvailability,
        searchForMovie: collection.searchOnAdd,
        tags: []
      };

      const {
        settings,
        validationErrors,
        validationWarnings
      } = selectSettings(collectionDefaults, pendingChanges, addError);

      return {
        isAdding,
        addError,
        isSmallScreen: dimensions.isSmallScreen,
        validationErrors,
        validationWarnings,
        isWindows: systemStatus.isWindows,
        ...settings
      };
    }
  );
}

const mapDispatchToProps = {
  addMovie,
  setMovieCollectionValue
};

class AddNewCollectionMovieModalContentConnector extends Component {

  //
  // Listeners

  onInputChange = ({ name, value }) => {
    this.props.setMovieCollectionValue({ name, value });
  };

  onAddMoviePress = () => {
    const {
      tmdbId,
      title,
      rootFolderPath,
      monitor,
      qualityProfileIds,
      minimumAvailability,
      searchForMovie,
      tags
    } = this.props;

    this.props.addMovie({
      tmdbId,
      title,
      rootFolderPath: rootFolderPath.value,
      monitor: monitor.value,
      qualityProfileIds: qualityProfileIds.value,
      minimumAvailability: minimumAvailability.value,
      searchForMovie: searchForMovie.value,
      tags: tags.value
    });

    this.props.onModalClose(true);
  };

  //
  // Render

  render() {
    return (
      <AddNewMovieModalContent
        {...this.props}
        onInputChange={this.onInputChange}
        onAddMoviePress={this.onAddMoviePress}
      />
    );
  }
}

AddNewCollectionMovieModalContentConnector.propTypes = {
  tmdbId: PropTypes.number.isRequired,
  title: PropTypes.string.isRequired,
  rootFolderPath: PropTypes.object,
  monitor: PropTypes.object.isRequired,
  qualityProfileIds: PropTypes.object,
  minimumAvailability: PropTypes.object.isRequired,
  searchForMovie: PropTypes.object.isRequired,
  tags: PropTypes.object.isRequired,
  onModalClose: PropTypes.func.isRequired,
  addMovie: PropTypes.func.isRequired,
  setMovieCollectionValue: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(AddNewCollectionMovieModalContentConnector);
