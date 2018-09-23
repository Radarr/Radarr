/* eslint max-params: 0 */
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { withRouter } from 'react-router-dom';
import { createSelector } from 'reselect';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import { saveDimensions, setIsSidebarVisible } from 'Store/Actions/appActions';
import { fetchCustomFilters } from 'Store/Actions/customFilterActions';
import { fetchArtist } from 'Store/Actions/artistActions';
import { fetchTags } from 'Store/Actions/tagActions';
import { fetchQualityProfiles, fetchLanguageProfiles, fetchMetadataProfiles, fetchUISettings, fetchImportLists } from 'Store/Actions/settingsActions';
import { fetchStatus } from 'Store/Actions/systemActions';
import ErrorPage from './ErrorPage';
import LoadingPage from './LoadingPage';
import Page from './Page';

function testLocalStorage() {
  const key = 'lidarrTest';

  try {
    localStorage.setItem(key, key);
    localStorage.removeItem(key);

    return true;
  } catch (e) {
    return false;
  }
}

function createMapStateToProps() {
  return createSelector(
    (state) => state.artist,
    (state) => state.customFilters,
    (state) => state.tags,
    (state) => state.settings,
    (state) => state.app,
    createDimensionsSelector(),
    (artist, customFilters, tags, settings, app, dimensions) => {
      const isPopulated = (
        artist.isPopulated &&
        customFilters.isPopulated &&
        tags.isPopulated &&
        settings.qualityProfiles.isPopulated &&
        settings.languageProfiles.isPopulated &&
        settings.metadataProfiles.isPopulated &&
        settings.importLists.isPopulated &&
        settings.ui.isPopulated
      );

      const hasError = !!(
        artist.error ||
        customFilters.error ||
        tags.error ||
        settings.qualityProfiles.error ||
        settings.languageProfiles.error ||
        settings.metadataProfiles.error ||
        settings.importLists.error ||
        settings.ui.error
      );

      return {
        isPopulated,
        hasError,
        artistError: artist.error,
        customFiltersError: tags.error,
        tagsError: tags.error,
        qualityProfilesError: settings.qualityProfiles.error,
        languageProfilesError: settings.languageProfiles.error,
        metadataProfilesError: settings.metadataProfiles.error,
        importListsError: settings.importLists.error,
        uiSettingsError: settings.ui.error,
        isSmallScreen: dimensions.isSmallScreen,
        isSidebarVisible: app.isSidebarVisible,
        version: app.version,
        isUpdated: app.isUpdated,
        isDisconnected: app.isDisconnected
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    dispatchFetchArtist() {
      dispatch(fetchArtist());
    },
    dispatchFetchCustomFilters() {
      dispatch(fetchCustomFilters());
    },
    dispatchFetchTags() {
      dispatch(fetchTags());
    },
    dispatchFetchQualityProfiles() {
      dispatch(fetchQualityProfiles());
    },
    dispatchFetchLanguageProfiles() {
      dispatch(fetchLanguageProfiles());
    },
    dispatchFetchMetadataProfiles() {
      dispatch(fetchMetadataProfiles());
    },
    dispatchFetchImportLists() {
      dispatch(fetchImportLists());
    },
    dispatchFetchUISettings() {
      dispatch(fetchUISettings());
    },
    dispatchFetchStatus() {
      dispatch(fetchStatus());
    },
    onResize(dimensions) {
      dispatch(saveDimensions(dimensions));
    },
    onSidebarVisibleChange(isSidebarVisible) {
      dispatch(setIsSidebarVisible({ isSidebarVisible }));
    }
  };
}

class PageConnector extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isLocalStorageSupported: testLocalStorage()
    };
  }

  componentDidMount() {
    if (!this.props.isPopulated) {
      this.props.dispatchFetchArtist();
      this.props.dispatchFetchCustomFilters();
      this.props.dispatchFetchTags();
      this.props.dispatchFetchQualityProfiles();
      this.props.dispatchFetchLanguageProfiles();
      this.props.dispatchFetchMetadataProfiles();
      this.props.dispatchFetchImportLists();
      this.props.dispatchFetchUISettings();
      this.props.dispatchFetchStatus();
    }
  }

  //
  // Listeners

  onSidebarToggle = () => {
    this.props.onSidebarVisibleChange(!this.props.isSidebarVisible);
  }

  //
  // Render

  render() {
    const {
      isPopulated,
      hasError,
      dispatchFetchArtist,
      dispatchFetchTags,
      dispatchFetchQualityProfiles,
      dispatchFetchLanguageProfiles,
      dispatchFetchMetadataProfiles,
      dispatchFetchImportLists,
      dispatchFetchUISettings,
      dispatchFetchStatus,
      ...otherProps
    } = this.props;

    if (hasError || !this.state.isLocalStorageSupported) {
      return (
        <ErrorPage
          {...this.state}
          {...otherProps}
        />
      );
    }

    if (isPopulated) {
      return (
        <Page
          {...otherProps}
          onSidebarToggle={this.onSidebarToggle}
        />
      );
    }

    return (
      <LoadingPage />
    );
  }
}

PageConnector.propTypes = {
  isPopulated: PropTypes.bool.isRequired,
  hasError: PropTypes.bool.isRequired,
  isSidebarVisible: PropTypes.bool.isRequired,
  dispatchFetchArtist: PropTypes.func.isRequired,
  dispatchFetchCustomFilters: PropTypes.func.isRequired,
  dispatchFetchTags: PropTypes.func.isRequired,
  dispatchFetchQualityProfiles: PropTypes.func.isRequired,
  dispatchFetchLanguageProfiles: PropTypes.func.isRequired,
  dispatchFetchMetadataProfiles: PropTypes.func.isRequired,
  dispatchFetchImportLists: PropTypes.func.isRequired,
  dispatchFetchUISettings: PropTypes.func.isRequired,
  dispatchFetchStatus: PropTypes.func.isRequired,
  onSidebarVisibleChange: PropTypes.func.isRequired
};

export default withRouter(
  connect(createMapStateToProps, createMapDispatchToProps)(PageConnector)
);
