import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchMetadataProfileSchema, saveMetadataProfile, setMetadataProfileValue } from 'Store/Actions/settingsActions';
import createProfileInUseSelector from 'Store/Selectors/createProfileInUseSelector';
import createProviderSettingsSelector from 'Store/Selectors/createProviderSettingsSelector';
import EditMetadataProfileModalContent from './EditMetadataProfileModalContent';

function createMapStateToProps() {
  return createSelector(
    createProviderSettingsSelector('metadataProfiles'),
    createProfileInUseSelector('metadataProfileId'),
    (metadataProfile, isInUse) => {
      return {
        ...metadataProfile,
        isInUse
      };
    }
  );
}

const mapDispatchToProps = {
  fetchMetadataProfileSchema,
  setMetadataProfileValue,
  saveMetadataProfile
};

class EditMetadataProfileModalContentConnector extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      dragIndex: null,
      dropIndex: null
    };
  }

  componentDidMount() {
    if (!this.props.id && !this.props.isPopulated) {
      this.props.fetchMetadataProfileSchema();
    }
  }

  componentDidUpdate(prevProps, prevState) {
    if (prevProps.isSaving && !this.props.isSaving && !this.props.saveError) {
      this.props.onModalClose();
    }
  }

  //
  // Listeners

  onInputChange = ({ name, value }) => {
    this.props.setMetadataProfileValue({ name, value });
  }

  onSavePress = () => {
    this.props.saveMetadataProfile({ id: this.props.id });
  }

  //
  // Render

  render() {
    return (
      <EditMetadataProfileModalContent
        {...this.state}
        {...this.props}
        onSavePress={this.onSavePress}
        onInputChange={this.onInputChange}
      />
    );
  }
}

EditMetadataProfileModalContentConnector.propTypes = {
  id: PropTypes.number,
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  item: PropTypes.object.isRequired,
  setMetadataProfileValue: PropTypes.func.isRequired,
  fetchMetadataProfileSchema: PropTypes.func.isRequired,
  saveMetadataProfile: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(EditMetadataProfileModalContentConnector);
