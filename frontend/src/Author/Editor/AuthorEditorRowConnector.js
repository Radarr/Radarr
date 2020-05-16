import PropTypes from 'prop-types';
import React from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createMetadataProfileSelector from 'Store/Selectors/createMetadataProfileSelector';
import createQualityProfileSelector from 'Store/Selectors/createQualityProfileSelector';
import AuthorEditorRow from './AuthorEditorRow';

function createMapStateToProps() {
  return createSelector(
    createMetadataProfileSelector(),
    createQualityProfileSelector(),
    (metadataProfile, qualityProfile) => {
      return {
        metadataProfile,
        qualityProfile
      };
    }
  );
}

function AuthorEditorRowConnector(props) {
  return (
    <AuthorEditorRow
      {...props}
    />
  );
}

AuthorEditorRowConnector.propTypes = {
  qualityProfileId: PropTypes.number.isRequired
};

export default connect(createMapStateToProps)(AuthorEditorRowConnector);
