import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import selectSettings from 'Store/Selectors/selectSettings';
import { setNetImportExclusionValue, saveNetImportExclusion } from 'Store/Actions/settingsActions';
import EditNetImportExclusionModalContent from './EditNetImportExclusionModalContent';

const newNetImportExclusion = {
  movieTitle: '',
  tmdbId: 0,
  movieYear: 0
};

function createNetImportExclusionSelector() {
  return createSelector(
    (state, { id }) => id,
    (state) => state.settings.netImportExclusions,
    (id, netImportExclusions) => {
      const {
        isFetching,
        error,
        isSaving,
        saveError,
        pendingChanges,
        items
      } = netImportExclusions;

      const mapping = id ? _.find(items, { id }) : newNetImportExclusion;
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
    createNetImportExclusionSelector(),
    (netImportExclusion) => {
      return {
        ...netImportExclusion
      };
    }
  );
}

const mapDispatchToProps = {
  setNetImportExclusionValue,
  saveNetImportExclusion
};

class EditNetImportExclusionModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    if (!this.props.id) {
      Object.keys(newNetImportExclusion).forEach((name) => {
        this.props.setNetImportExclusionValue({
          name,
          value: newNetImportExclusion[name]
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
    this.props.setNetImportExclusionValue({ name, value });
  }

  onSavePress = () => {
    this.props.saveNetImportExclusion({ id: this.props.id });
  }

  //
  // Render

  render() {
    return (
      <EditNetImportExclusionModalContent
        {...this.props}
        onSavePress={this.onSavePress}
        onInputChange={this.onInputChange}
      />
    );
  }
}

EditNetImportExclusionModalContentConnector.propTypes = {
  id: PropTypes.number,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  item: PropTypes.object.isRequired,
  setNetImportExclusionValue: PropTypes.func.isRequired,
  saveNetImportExclusion: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(EditNetImportExclusionModalContentConnector);
