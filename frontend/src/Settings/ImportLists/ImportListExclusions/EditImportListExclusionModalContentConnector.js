import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { saveImportExclusion, setImportExclusionValue } from 'Store/Actions/settingsActions';
import selectSettings from 'Store/Selectors/selectSettings';
import EditImportListExclusionModalContent from './EditImportListExclusionModalContent';

const newImportExclusion = {
  movieTitle: '',
  tmdbId: 0,
  movieYear: 0
};

function createImportExclusionSelector() {
  return createSelector(
    (state, { id }) => id,
    (state) => state.settings.importExclusions,
    (id, importExclusions) => {
      const {
        isFetching,
        error,
        isSaving,
        saveError,
        pendingChanges,
        items
      } = importExclusions;

      const mapping = id ? _.find(items, { id }) : newImportExclusion;
      const settings = selectSettings(mapping, pendingChanges, saveError);

      return {
        id,
        isFetching,
        error,
        isSaving,
        saveError,
        item: settings.settings,
        ...settings
      };
    }
  );
}

function createMapStateToProps() {
  return createSelector(
    createImportExclusionSelector(),
    (importExclusion) => {
      return {
        ...importExclusion
      };
    }
  );
}

const mapDispatchToProps = {
  setImportExclusionValue,
  saveImportExclusion
};

class EditImportExclusionModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    if (!this.props.id) {
      Object.keys(newImportExclusion).forEach((name) => {
        this.props.setImportExclusionValue({
          name,
          value: newImportExclusion[name]
        });
      });
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
    this.props.setImportExclusionValue({ name, value });
  }

  onSavePress = () => {
    this.props.saveImportExclusion({ id: this.props.id });
  }

  //
  // Render

  render() {
    return (
      <EditImportListExclusionModalContent
        {...this.props}
        onSavePress={this.onSavePress}
        onInputChange={this.onInputChange}
      />
    );
  }
}

EditImportExclusionModalContentConnector.propTypes = {
  id: PropTypes.number,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  item: PropTypes.object.isRequired,
  setImportExclusionValue: PropTypes.func.isRequired,
  saveImportExclusion: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(EditImportExclusionModalContentConnector);
