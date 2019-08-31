import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createProviderSettingsSelector from 'Store/Selectors/createProviderSettingsSelector';
import { setCustomFormatValue, saveCustomFormat } from 'Store/Actions/settingsActions';
import EditCustomFormatModalContent from './EditCustomFormatModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.advancedSettings,
    createProviderSettingsSelector('customFormats'),
    (advancedSettings, customFormat) => {
      return {
        advancedSettings,
        ...customFormat
      };
    }
  );
}

const mapDispatchToProps = {
  setCustomFormatValue,
  saveCustomFormat
};

class EditCustomFormatModalContentConnector extends Component {

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
    this.props.setCustomFormatValue({ name, value });
  }

  onSavePress = () => {
    this.props.saveCustomFormat({ id: this.props.id });
  }

  //
  // Render

  render() {
    return (
      <EditCustomFormatModalContent
        {...this.props}
        onSavePress={this.onSavePress}
        onInputChange={this.onInputChange}
      />
    );
  }
}

EditCustomFormatModalContentConnector.propTypes = {
  id: PropTypes.number,
  isFetching: PropTypes.bool.isRequired,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  item: PropTypes.object.isRequired,
  setCustomFormatValue: PropTypes.func.isRequired,
  saveCustomFormat: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(EditCustomFormatModalContentConnector);
