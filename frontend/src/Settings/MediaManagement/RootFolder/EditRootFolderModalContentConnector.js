import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { saveRootFolder, setRootFolderValue } from 'Store/Actions/settingsActions';
import createProviderSettingsSelector from 'Store/Selectors/createProviderSettingsSelector';
import EditRootFolderModalContent from './EditRootFolderModalContent';

function createMapStateToProps() {
  return createSelector(
    (state, { id }) => id,
    (state) => state.settings.advancedSettings,
    (state) => state.settings.metadataProfiles,
    (state) => state.settings.rootFolders,
    createProviderSettingsSelector('rootFolders'),
    (id, advancedSettings, metadataProfiles, rootFolders, rootFolderSettings) => {
      return {
        advancedSettings,
        showMetadataProfile: metadataProfiles.items.length > 1,
        ...rootFolderSettings,
        isFetching: rootFolders.isFetching
      };
    }
  );
}

const mapDispatchToProps = {
  setRootFolderValue,
  saveRootFolder
};

class EditRootFolderModalContentConnector extends Component {

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
    this.props.setRootFolderValue({ name, value });
  }

  onSavePress = () => {
    this.props.saveRootFolder({ id: this.props.id });

    if (this.props.onRootFolderAdded) {
      this.props.onRootFolderAdded(this.props.item.path);
    }
  }

  //
  // Render

  render() {
    return (
      <EditRootFolderModalContent
        {...this.props}
        onSavePress={this.onSavePress}
        onInputChange={this.onInputChange}
      />
    );
  }
}

EditRootFolderModalContentConnector.propTypes = {
  id: PropTypes.number,
  isFetching: PropTypes.bool.isRequired,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  item: PropTypes.object.isRequired,
  setRootFolderValue: PropTypes.func.isRequired,
  saveRootFolder: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired,
  onRootFolderAdded: PropTypes.func
};

export default connect(createMapStateToProps, mapDispatchToProps)(EditRootFolderModalContentConnector);
