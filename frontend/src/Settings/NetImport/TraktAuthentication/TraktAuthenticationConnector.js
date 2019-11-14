import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import createSettingsSectionSelector from 'Store/Selectors/createSettingsSectionSelector';
import { fetchTraktAuthentication, setTraktAuthenticationValue, saveTraktAuthentication } from 'Store/Actions/settingsActions';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import TraktAuthentication from './TraktAuthentication';

const SECTION = 'traktAuthentication';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.advancedSettings,
    createSettingsSectionSelector(SECTION),
    (advancedSettings, sectionSettings) => {
      return {
        advancedSettings,
        ...sectionSettings
      };
    }
  );
}

const mapDispatchToProps = {
  dispatchFetchTraktAuthentication: fetchTraktAuthentication,
  dispatchSetTraktAuthenticationValue: setTraktAuthenticationValue,
  dispatchSaveTraktAuthentication: saveTraktAuthentication,
  dispatchClearPendingChanges: clearPendingChanges
};

class TraktAuthenticationConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      dispatchFetchTraktAuthentication,
      dispatchSaveTraktAuthentication,
      onChildMounted
    } = this.props;

    dispatchFetchTraktAuthentication();
    onChildMounted(dispatchSaveTraktAuthentication);
  }

  componentDidUpdate(prevProps) {
    const {
      hasPendingChanges,
      isSaving,
      onChildStateChange
    } = this.props;

    if (
      prevProps.isSaving !== isSaving ||
      prevProps.hasPendingChanges !== hasPendingChanges
    ) {
      onChildStateChange({
        isSaving,
        hasPendingChanges
      });
    }
  }

  componentWillUnmount() {
    this.props.dispatchClearPendingChanges({ section: SECTION });
  }

  //
  // Listeners

  onInputChange = ({ name, value }) => {
    this.props.dispatchSetTraktAuthenticationValue({ name, value });
  }

  //
  // Render

  render() {
    return (
      <TraktAuthentication
        onInputChange={this.onInputChange}
        {...this.props}
      />
    );
  }
}

TraktAuthenticationConnector.propTypes = {
  isSaving: PropTypes.bool.isRequired,
  hasPendingChanges: PropTypes.bool.isRequired,
  dispatchFetchTraktAuthentication: PropTypes.func.isRequired,
  dispatchSetTraktAuthenticationValue: PropTypes.func.isRequired,
  dispatchSaveTraktAuthentication: PropTypes.func.isRequired,
  dispatchClearPendingChanges: PropTypes.func.isRequired,
  onChildMounted: PropTypes.func.isRequired,
  onChildStateChange: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(TraktAuthenticationConnector);
