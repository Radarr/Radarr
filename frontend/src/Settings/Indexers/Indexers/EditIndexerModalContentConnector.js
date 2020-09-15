import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { saveIndexer, setIndexerFieldValue, setIndexerValue, testIndexer } from 'Store/Actions/settingsActions';
import createProviderSettingsSelector from 'Store/Selectors/createProviderSettingsSelector';
import createSettingsSectionSelector from 'Store/Selectors/createSettingsSectionSelector';
import getJackettIndexers from 'Utilities/getJackettIndexers';
import translate from 'Utilities/String/translate';
import EditIndexerModalContent from './EditIndexerModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.advancedSettings,
    createProviderSettingsSelector('indexers'),
    createSettingsSectionSelector('indexerOptions'),
    (advancedSettings, indexer, sectionSettings) => {
      let jackettIndexerOptions = null;

      if (sectionSettings.settings.jackettApi.value && sectionSettings.settings.jackettPath.value) {
        jackettIndexerOptions = [{ key: 'default', value: translate('JackettNoIndexersConfigured') }];
        const jackettIndexers = getJackettIndexers(sectionSettings.settings.jackettApi.value, sectionSettings.settings.jackettPath.value);

        if (jackettIndexers.configuredIndexers?.length) {
          jackettIndexerOptions = [{ key: 'default', value: translate('JackettSelectOne') }];
          jackettIndexers.configuredIndexers.forEach((jackettIndexer) => {
            jackettIndexerOptions.push({ key: JSON.stringify({name: jackettIndexer.name, id: jackettIndexer.id}), value: jackettIndexer.name });
          });
        }
      }

      return {
        advancedSettings,
        jackettIndexerOptions,
        jackettPath: sectionSettings.settings.jackettPath.value,
        jackettApi: sectionSettings.settings.jackettApi.value,
        ...indexer
      };
    }
  );
}

const mapDispatchToProps = {
  setIndexerValue,
  setIndexerFieldValue,
  saveIndexer,
  testIndexer
};

class EditIndexerModalContentConnector extends Component {

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
    this.props.setIndexerValue({ name, value });
  }

  onFieldChange = ({ name, value }) => {
    this.props.setIndexerFieldValue({ name, value });
  }

  onSavePress = () => {
    this.props.saveIndexer({ id: this.props.id });
  }

  onTestPress = () => {
    this.props.testIndexer({ id: this.props.id });
  }

  onJackettIndexerChange = ({ name, value }) => {
    if (name !== 'default') {
      const indexer = JSON.parse(value);
      this.props.setIndexerValue({ name: 'name', value: indexer.name });
      this.props.setIndexerFieldValue({ name: 'baseUrl', value: `${this.props.jackettPath}/api/v2.0/indexers/${indexer.id}/results/torznab/` });
      this.props.setIndexerFieldValue({ name: 'apiKey', value: this.props.jackettApi });
    }
  }

  //
  // Render

  render() {
    return (
      <EditIndexerModalContent
        {...this.props}
        onSavePress={this.onSavePress}
        onTestPress={this.onTestPress}
        onInputChange={this.onInputChange}
        onFieldChange={this.onFieldChange}
        onJackettIndexerChange={this.onJackettIndexerChange}
      />
    );
  }
}

EditIndexerModalContentConnector.propTypes = {
  id: PropTypes.number,
  isFetching: PropTypes.bool.isRequired,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  item: PropTypes.object.isRequired,
  setIndexerValue: PropTypes.func.isRequired,
  setIndexerFieldValue: PropTypes.func.isRequired,
  saveIndexer: PropTypes.func.isRequired,
  testIndexer: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired,
  jackettPath: PropTypes.string,
  jackettApi: PropTypes.string
};

export default connect(createMapStateToProps, mapDispatchToProps)(EditIndexerModalContentConnector);
