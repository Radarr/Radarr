import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { saveMovieCollection, setMovieCollectionValue } from 'Store/Actions/movieCollectionActions';
import createCollectionSelector from 'Store/Selectors/createCollectionSelector';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import selectSettings from 'Store/Selectors/selectSettings';
import EditCollectionModalContent from './EditCollectionModalContent';

function createIsPathChangingSelector() {
  return createSelector(
    (state) => state.movieCollections.pendingChanges,
    createCollectionSelector(),
    (pendingChanges, collection) => {
      const rootFolderPath = pendingChanges.rootFolderPath;

      if (rootFolderPath == null) {
        return false;
      }

      return collection.rootFolderPath !== rootFolderPath;
    }
  );
}

function createMapStateToProps() {
  return createSelector(
    (state) => state.movieCollections,
    createCollectionSelector(),
    createIsPathChangingSelector(),
    createDimensionsSelector(),
    (moviesState, collection, isPathChanging, dimensions) => {
      const {
        isSaving,
        saveError,
        pendingChanges
      } = moviesState;

      const movieSettings = {
        monitored: collection.monitored,
        qualityProfileId: collection.qualityProfileId,
        minimumAvailability: collection.minimumAvailability,
        rootFolderPath: collection.rootFolderPath,
        searchOnAdd: collection.searchOnAdd
      };

      const settings = selectSettings(movieSettings, pendingChanges, saveError);

      return {
        title: collection.title,
        images: collection.images,
        overview: collection.overview,
        isSaving,
        saveError,
        isPathChanging,
        originalPath: collection.path,
        item: settings.settings,
        isSmallScreen: dimensions.isSmallScreen,
        ...settings
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchSetMovieCollectionValue: setMovieCollectionValue,
  dispatchSaveMovieCollection: saveMovieCollection
};

class EditCollectionModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidUpdate(prevProps, prevState) {
    if (prevProps.isSaving && !this.props.isSaving && !this.props.saveError) {
      this.props.onModalClose();
    }
  }

  //
  // Listeners

  onInputChange = ({ name, value }) => {
    this.props.dispatchSetMovieCollectionValue({ name, value });
  };

  onSavePress = () => {
    this.props.dispatchSaveMovieCollection({
      id: this.props.collectionId
    });
  };

  //
  // Render

  render() {
    return (
      <EditCollectionModalContent
        {...this.props}
        onInputChange={this.onInputChange}
        onSavePress={this.onSavePress}
        onMoveMoviePress={this.onMoveMoviePress}
      />
    );
  }
}

EditCollectionModalContentConnector.propTypes = {
  collectionId: PropTypes.number,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  dispatchSetMovieCollectionValue: PropTypes.func.isRequired,
  dispatchSaveMovieCollection: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(EditCollectionModalContentConnector);
