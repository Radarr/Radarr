import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { saveNetImport, setNetImportFieldValue, setNetImportValue, testNetImport } from 'Store/Actions/settingsActions';
import createProviderSettingsSelector from 'Store/Selectors/createProviderSettingsSelector';
import EditNetImportModalContent from './EditNetImportModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.advancedSettings,
    createProviderSettingsSelector('netImports'),
    (advancedSettings, netImport) => {
      return {
        advancedSettings,
        ...netImport
      };
    }
  );
}

const mapDispatchToProps = {
  setNetImportValue,
  setNetImportFieldValue,
  saveNetImport,
  testNetImport
};

class EditNetImportModalContentConnector extends Component {

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
    this.props.setNetImportValue({ name, value });
  }

  onFieldChange = ({ name, value }) => {
    this.props.setNetImportFieldValue({ name, value });
  }

  onSavePress = () => {
    this.props.saveNetImport({ id: this.props.id });
  }

  onTestPress = () => {
    this.props.testNetImport({ id: this.props.id });
  }

  //
  // Render

  render() {
    return (
      <EditNetImportModalContent
        {...this.props}
        onSavePress={this.onSavePress}
        onTestPress={this.onTestPress}
        onInputChange={this.onInputChange}
        onFieldChange={this.onFieldChange}
      />
    );
  }
}

EditNetImportModalContentConnector.propTypes = {
  id: PropTypes.number,
  isFetching: PropTypes.bool.isRequired,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  item: PropTypes.object.isRequired,
  setNetImportValue: PropTypes.func.isRequired,
  setNetImportFieldValue: PropTypes.func.isRequired,
  saveNetImport: PropTypes.func.isRequired,
  testNetImport: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(EditNetImportModalContentConnector);
