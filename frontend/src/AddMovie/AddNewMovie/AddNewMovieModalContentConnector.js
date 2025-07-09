import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { addMovie, setAddMovieDefault } from 'Store/Actions/addMovieActions';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import createSettingsSectionSelector from 'Store/Selectors/createSettingsSectionSelector';
import createSystemStatusSelector from 'Store/Selectors/createSystemStatusSelector';
import selectSettings from 'Store/Selectors/selectSettings';
import AddNewMovieModalContent from './AddNewMovieModalContent';

function createMapStateToProps() {
  const selectMediaManagementSettings =
    createSettingsSectionSelector('mediaManagement');
  return createSelector(
    (state) => state.addMovie,
    createDimensionsSelector(),
    createSystemStatusSelector(),
    selectMediaManagementSettings,
    (addMovieState, dimensions, systemStatus, mediaManagementSettings) => {
      const { isAdding, addError, defaults } = addMovieState;

      const { settings, validationErrors, validationWarnings } = selectSettings(
        defaults,
        {},
        addError
      );

      return {
        isAdding,
        addError,
        isSmallScreen: dimensions.isSmallScreen,
        validationErrors,
        validationWarnings,
        isWindows: systemStatus.isWindows,
        searchForMovieDefaultOverride:
          mediaManagementSettings.settings.searchForMovieDefaultOverride
            ?.value || 'default',
        ...settings
      };
    }
  );
}

const mapDispatchToProps = {
  setAddMovieDefault,
  addMovie
};

class AddNewMovieModalContentConnector extends Component {
  //
  // Listeners

  onInputChange = ({ name, value }) => {
    this.props.setAddMovieDefault({ [name]: value });
  };

  onAddMoviePress = () => {
    const {
      tmdbId,
      rootFolderPath,
      monitor,
      qualityProfileId,
      minimumAvailability,
      searchForMovie,
      tags
    } = this.props;

    this.props.addMovie({
      tmdbId,
      rootFolderPath: rootFolderPath.value,
      monitor: monitor.value,
      qualityProfileId: qualityProfileId.value,
      minimumAvailability: minimumAvailability.value,
      searchForMovie: searchForMovie.value,
      tags: tags.value
    });
  };

  //
  // Render

  render() {
    return (
      <AddNewMovieModalContent
        {...this.props}
        searchForMovieDefaultOverride={this.props.searchForMovieDefaultOverride}
        onInputChange={this.onInputChange}
        onAddMoviePress={this.onAddMoviePress}
      />
    );
  }
}

AddNewMovieModalContentConnector.propTypes = {
  tmdbId: PropTypes.number.isRequired,
  rootFolderPath: PropTypes.object,
  monitor: PropTypes.object.isRequired,
  qualityProfileId: PropTypes.object,
  minimumAvailability: PropTypes.object.isRequired,
  searchForMovie: PropTypes.object.isRequired,
  tags: PropTypes.object.isRequired,
  onModalClose: PropTypes.func.isRequired,
  setAddMovieDefault: PropTypes.func.isRequired,
  addMovie: PropTypes.func.isRequired,
  searchForMovieDefaultOverride: PropTypes.string.isRequired
};

export default connect(
  createMapStateToProps,
  mapDispatchToProps
)(AddNewMovieModalContentConnector);
