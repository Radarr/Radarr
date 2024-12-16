import React from 'react';
import { Error } from 'App/State/AppSectionState';
import getErrorMessage from 'Utilities/Object/getErrorMessage';
import styles from './ErrorPage.css';

interface ErrorPageProps {
  version: string;
  isLocalStorageSupported: boolean;
  translationsError?: Error;
  moviesError?: Error;
  customFiltersError?: Error;
  tagsError?: Error;
  qualityProfilesError?: Error;
  languagesError?: Error;
  uiSettingsError?: Error;
  systemStatusError?: Error;
}

function ErrorPage(props: ErrorPageProps) {
  const {
    version,
    isLocalStorageSupported,
    translationsError,
    moviesError,
    customFiltersError,
    tagsError,
    qualityProfilesError,
    languagesError,
    uiSettingsError,
    systemStatusError,
  } = props;

  let errorMessage = 'Failed to load Radarr';

  if (!isLocalStorageSupported) {
    errorMessage =
      'Local Storage is not supported or disabled. A plugin or private browsing may have disabled it.';
  } else if (translationsError) {
    errorMessage = getErrorMessage(
      translationsError,
      'Failed to load translations from API'
    );
  } else if (moviesError) {
    errorMessage = getErrorMessage(
      moviesError,
      'Failed to load movie from API'
    );
  } else if (customFiltersError) {
    errorMessage = getErrorMessage(
      customFiltersError,
      'Failed to load custom filters from API'
    );
  } else if (tagsError) {
    errorMessage = getErrorMessage(tagsError, 'Failed to load tags from API');
  } else if (qualityProfilesError) {
    errorMessage = getErrorMessage(
      qualityProfilesError,
      'Failed to load quality profiles from API'
    );
  } else if (languagesError) {
    errorMessage = getErrorMessage(
      languagesError,
      'Failed to load languages from API'
    );
  } else if (uiSettingsError) {
    errorMessage = getErrorMessage(
      uiSettingsError,
      'Failed to load UI settings from API'
    );
  } else if (systemStatusError) {
    errorMessage = getErrorMessage(
      uiSettingsError,
      'Failed to load system status from API'
    );
  }

  return (
    <div className={styles.page}>
      <div>{errorMessage}</div>

      <div className={styles.version}>Version {version}</div>
    </div>
  );
}

export default ErrorPage;
