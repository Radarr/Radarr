import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { createSelector } from 'reselect';
import createProfileInUseSelector from 'Store/Selectors/createProfileInUseSelector';
import createProviderSettingsSelector from 'Store/Selectors/createProviderSettingsSelector';
import { fetchMetadataProfileSchema, setMetadataProfileValue, saveMetadataProfile } from 'Store/Actions/settingsActions';
import connectSection from 'Store/connectSection';
import EditMetadataProfileModalContent from './EditMetadataProfileModalContent';

function createPrimaryAlbumTypesSelector() {
  return createSelector(
    createProviderSettingsSelector(),
    (metadataProfile) => {
      const primaryAlbumTypes = metadataProfile.item.primaryAlbumTypes;
      if (!primaryAlbumTypes || !primaryAlbumTypes.value) {
        return [];
      }

      return _.reduceRight(primaryAlbumTypes.value, (result, { allowed, albumType }) => {
        if (allowed) {
          result.push({
            key: albumType.id,
            value: albumType.name
          });
        }

        return result;
      }, []);
    }
  );
}

function createSecondaryAlbumTypesSelector() {
  return createSelector(
    createProviderSettingsSelector(),
    (metadataProfile) => {
      const secondaryAlbumTypes = metadataProfile.item.secondaryAlbumTypes;
      if (!secondaryAlbumTypes || !secondaryAlbumTypes.value) {
        return [];
      }

      return _.reduceRight(secondaryAlbumTypes.value, (result, { allowed, albumType }) => {
        if (allowed) {
          result.push({
            key: albumType.id,
            value: albumType.name
          });
        }

        return result;
      }, []);
    }
  );
}

function createMapStateToProps() {
  return createSelector(
    createProviderSettingsSelector(),
    createPrimaryAlbumTypesSelector(),
    createSecondaryAlbumTypesSelector(),
    createProfileInUseSelector('metadataProfileId'),
    (metadataProfile, primaryAlbumTypes, secondaryAlbumTypes, isInUse) => {
      return {
        primaryAlbumTypes,
        secondaryAlbumTypes,
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

  onMetadataPrimaryTypeItemAllowedChange = (id, allowed) => {
    const metadataProfile = _.cloneDeep(this.props.item);

    const item = _.find(metadataProfile.primaryAlbumTypes.value, (i) => i.albumType.id === id);
    item.allowed = allowed;

    this.props.setMetadataProfileValue({
      name: 'primaryAlbumTypes',
      value: metadataProfile.primaryAlbumTypes.value
    });
  }

  onMetadataSecondaryTypeItemAllowedChange = (id, allowed) => {
    const metadataProfile = _.cloneDeep(this.props.item);

    const item = _.find(metadataProfile.secondaryAlbumTypes.value, (i) => i.albumType.id === id);
    item.allowed = allowed;

    this.props.setMetadataProfileValue({
      name: 'secondaryAlbumTypes',
      value: metadataProfile.secondaryAlbumTypes.value
    });
  }

  //
  // Render

  render() {
    if (_.isEmpty(this.props.item.primaryAlbumTypes) && !this.props.isFetching) {
      return null;
    }

    return (
      <EditMetadataProfileModalContent
        {...this.state}
        {...this.props}
        onSavePress={this.onSavePress}
        onInputChange={this.onInputChange}
        onMetadataPrimaryTypeItemAllowedChange={this.onMetadataPrimaryTypeItemAllowedChange}
        onMetadataSecondaryTypeItemAllowedChange={this.onMetadataSecondaryTypeItemAllowedChange}
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

export default connectSection(
  createMapStateToProps,
  mapDispatchToProps,
  undefined,
  undefined,
  { section: 'metadataProfiles' }
)(EditMetadataProfileModalContentConnector);
