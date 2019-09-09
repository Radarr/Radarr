import PropTypes from 'prop-types';
import React from 'react';
import getErrorMessage from 'Utilities/Object/getErrorMessage';
import styles from './ErrorPage.css';

function ErrorPage(props) {
  const {
    version,
    isLocalStorageSupported,
    artistError,
    customFiltersError,
    tagsError,
    qualityProfilesError,
    metadataProfilesError,
    uiSettingsError,
    systemStatusError
  } = props;

  let errorMessage = 'Failed to load Lidarr';

  if (!isLocalStorageSupported) {
    errorMessage = 'Local Storage is not supported or disabled. A plugin or private browsing may have disabled it.';
  } else if (artistError) {
    errorMessage = getErrorMessage(artistError, 'Failed to load artist from API');
  } else if (customFiltersError) {
    errorMessage = getErrorMessage(customFiltersError, 'Failed to load custom filters from API');
  } else if (tagsError) {
    errorMessage = getErrorMessage(tagsError, 'Failed to load tags from API');
  } else if (qualityProfilesError) {
    errorMessage = getErrorMessage(qualityProfilesError, 'Failed to load quality profiles from API');
  } else if (metadataProfilesError) {
    errorMessage = getErrorMessage(metadataProfilesError, 'Failed to load metadata profiles from API');
  } else if (uiSettingsError) {
    errorMessage = getErrorMessage(uiSettingsError, 'Failed to load UI settings from API');
  } else if (systemStatusError) {
    errorMessage = getErrorMessage(uiSettingsError, 'Failed to load system status from API');
  }

  return (
    <div className={styles.page}>
      <div className={styles.errorMessage}>
        {errorMessage}
      </div>

      <div className={styles.version}>
        Version {version}
      </div>
    </div>
  );
}

ErrorPage.propTypes = {
  version: PropTypes.string.isRequired,
  isLocalStorageSupported: PropTypes.bool.isRequired,
  artistError: PropTypes.object,
  customFiltersError: PropTypes.object,
  tagsError: PropTypes.object,
  qualityProfilesError: PropTypes.object,
  metadataProfilesError: PropTypes.object,
  uiSettingsError: PropTypes.object,
  systemStatusError: PropTypes.object
};

export default ErrorPage;
