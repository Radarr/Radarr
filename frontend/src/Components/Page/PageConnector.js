import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { withRouter } from 'react-router-dom';
import { createSelector } from 'reselect';
import createDimensionsSelector from 'Store/Selectors/createDimensionsSelector';
import { saveDimensions, setIsSidebarVisible } from 'Store/Actions/appActions';
import { fetchCustomFilters } from 'Store/Actions/customFilterActions';
import { fetchMovies } from 'Store/Actions/movieActions';
import { fetchTags } from 'Store/Actions/tagActions';
import { fetchQualityProfiles, fetchUISettings } from 'Store/Actions/settingsActions';
import { fetchStatus } from 'Store/Actions/systemActions';
import ErrorPage from './ErrorPage';
import LoadingPage from './LoadingPage';
import Page from './Page';

function testLocalStorage() {
  const key = 'radarrTest';

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
    (state) => state.movies,
    (state) => state.customFilters,
    (state) => state.tags,
    (state) => state.settings.ui,
    (state) => state.settings.qualityProfiles,
    (state) => state.app,
    createDimensionsSelector(),
    (
      movies,
      customFilters,
      tags,
      uiSettings,
      qualityProfiles,
      app,
      dimensions
    ) => {
      const isPopulated = (
        movies.isPopulated &&
        customFilters.isPopulated &&
        tags.isPopulated &&
        qualityProfiles.isPopulated &&
        uiSettings.isPopulated
      );

      const hasError = !!(
        movies.error ||
        customFilters.error ||
        tags.error ||
        qualityProfiles.error ||
        uiSettings.error
      );

      return {
        isPopulated,
        hasError,
        moviesError: movies.error,
        customFiltersError: tags.error,
        tagsError: tags.error,
        qualityProfilesError: qualityProfiles.error,
        uiSettingsError: uiSettings.error,
        isSmallScreen: dimensions.isSmallScreen,
        isSidebarVisible: app.isSidebarVisible,
        enableColorImpairedMode: uiSettings.item.enableColorImpairedMode,
        version: app.version,
        isUpdated: app.isUpdated,
        isDisconnected: app.isDisconnected
      };
    }
  );
}

function createMapDispatchToProps(dispatch, props) {
  return {
    dispatchFetchMovies() {
      dispatch(fetchMovies());
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
      this.props.dispatchFetchMovies();
      this.props.dispatchFetchCustomFilters();
      this.props.dispatchFetchTags();
      this.props.dispatchFetchQualityProfiles();
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
      dispatchFetchMovies,
      dispatchFetchTags,
      dispatchFetchQualityProfiles,
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
  dispatchFetchMovies: PropTypes.func.isRequired,
  dispatchFetchCustomFilters: PropTypes.func.isRequired,
  dispatchFetchTags: PropTypes.func.isRequired,
  dispatchFetchQualityProfiles: PropTypes.func.isRequired,
  dispatchFetchUISettings: PropTypes.func.isRequired,
  dispatchFetchStatus: PropTypes.func.isRequired,
  onSidebarVisibleChange: PropTypes.func.isRequired
};

export default withRouter(
  connect(createMapStateToProps, createMapDispatchToProps)(PageConnector)
);
