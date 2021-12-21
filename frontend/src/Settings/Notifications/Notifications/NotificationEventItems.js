import PropTypes from 'prop-types';
import React from 'react';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormLabel from 'Components/Form/FormLabel';
import { inputTypes } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './NotificationEventItems.css';

function NotificationEventItems(props) {
  const {
    item,
    onInputChange
  } = props;

  const {
    onGrab,
    onDownload,
    onUpgrade,
    onRename,
    onMovieDelete,
    onMovieFileDelete,
    onMovieFileDeleteForUpgrade,
    onHealthIssue,
    onApplicationUpdate,
    supportsOnGrab,
    supportsOnDownload,
    supportsOnUpgrade,
    supportsOnRename,
    supportsOnMovieDelete,
    supportsOnMovieFileDelete,
    supportsOnMovieFileDeleteForUpgrade,
    supportsOnApplicationUpdate,
    supportsOnHealthIssue,
    includeHealthWarnings
  } = item;

  return (
    <FormGroup>
      <FormLabel>{translate('NotificationTriggers')}</FormLabel>
      <div>
        <FormInputHelpText
          text={translate('NotificationTriggersHelpText')}
          link="https://wiki.servarr.com/radarr/settings#connections"
        />
        <div className={styles.events}>
          <div>
            <FormInputGroup
              type={inputTypes.CHECK}
              name="onGrab"
              helpText={translate('OnGrabHelpText')}
              isDisabled={!supportsOnGrab.value}
              {...onGrab}
              onChange={onInputChange}
            />
          </div>

          <div>
            <FormInputGroup
              type={inputTypes.CHECK}
              name="onDownload"
              helpText={translate('OnDownloadHelpText')}
              isDisabled={!supportsOnDownload.value}
              {...onDownload}
              onChange={onInputChange}
            />
          </div>

          {
            onDownload.value &&
              <div>
                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="onUpgrade"
                  helpText={translate('OnUpgradeHelpText')}
                  isDisabled={!supportsOnUpgrade.value}
                  {...onUpgrade}
                  onChange={onInputChange}
                />
              </div>
          }

          <div>
            <FormInputGroup
              type={inputTypes.CHECK}
              name="onRename"
              helpText={translate('OnRenameHelpText')}
              isDisabled={!supportsOnRename.value}
              {...onRename}
              onChange={onInputChange}
            />
          </div>

          <div>
            <FormInputGroup
              type={inputTypes.CHECK}
              name="onMovieDelete"
              helpText={translate('OnMovieDeleteHelpText')}
              isDisabled={!supportsOnMovieDelete.value}
              {...onMovieDelete}
              onChange={onInputChange}
            />
          </div>

          <div>
            <FormInputGroup
              type={inputTypes.CHECK}
              name="onMovieFileDelete"
              helpText={translate('OnMovieFileDeleteHelpText')}
              isDisabled={!supportsOnMovieFileDelete.value}
              {...onMovieFileDelete}
              onChange={onInputChange}
            />
          </div>

          {
            onMovieFileDelete.value &&
              <div>
                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="onMovieFileDeleteForUpgrade"
                  helpText={translate('OnMovieFileDeleteForUpgradeHelpText')}
                  isDisabled={!supportsOnMovieFileDeleteForUpgrade.value}
                  {...onMovieFileDeleteForUpgrade}
                  onChange={onInputChange}
                />
              </div>
          }

          <div>
            <FormInputGroup
              type={inputTypes.CHECK}
              name="onHealthIssue"
              helpText={translate('OnHealthIssueHelpText')}
              isDisabled={!supportsOnHealthIssue.value}
              {...onHealthIssue}
              onChange={onInputChange}
            />
          </div>

          {
            onHealthIssue.value &&
              <div>
                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="includeHealthWarnings"
                  helpText={translate('IncludeHealthWarningsHelpText')}
                  isDisabled={!supportsOnHealthIssue.value}
                  {...includeHealthWarnings}
                  onChange={onInputChange}
                />
              </div>
          }

          <div>
            <FormInputGroup
              type={inputTypes.CHECK}
              name="onApplicationUpdate"
              helpText={translate('OnApplicationUpdateHelpText')}
              isDisabled={!supportsOnApplicationUpdate.value}
              {...onApplicationUpdate}
              onChange={onInputChange}
            />
          </div>
        </div>
      </div>
    </FormGroup>
  );
}

NotificationEventItems.propTypes = {
  item: PropTypes.object.isRequired,
  onInputChange: PropTypes.func.isRequired
};

export default NotificationEventItems;
