import PropTypes from 'prop-types';
import React from 'react';
import { inputTypes } from 'Helpers/Props';
import FormGroup from 'Components/Form/FormGroup';
import FormLabel from 'Components/Form/FormLabel';
import FormInputHelpText from 'Components/Form/FormInputHelpText';
import FormInputGroup from 'Components/Form/FormInputGroup';
import styles from './NotificationEventItems.css';

function NotificationEventItems(props) {
  const {
    item,
    onInputChange
  } = props;

  const {
    onGrab,
    onReleaseImport,
    onUpgrade,
    onRename,
    onHealthIssue,
    onDownloadFailure,
    onImportFailure,
    onTrackRetag,
    supportsOnGrab,
    supportsOnReleaseImport,
    supportsOnUpgrade,
    supportsOnRename,
    supportsOnHealthIssue,
    includeHealthWarnings,
    supportsOnDownloadFailure,
    supportsOnImportFailure,
    supportsOnTrackRetag
  } = item;

  return (
    <FormGroup>
      <FormLabel>Notification Triggers</FormLabel>
      <div>
        <FormInputHelpText
          text="Select which events should trigger this notification"
          link="https://github.com/lidarr/Lidarr/wiki/Connections"
        />
        <div className={styles.events}>
          <div>
            <FormInputGroup
              type={inputTypes.CHECK}
              name="onGrab"
              helpText="On Grab"
              isDisabled={!supportsOnGrab.value}
              {...onGrab}
              onChange={onInputChange}
            />
          </div>

          <div>
            <FormInputGroup
              type={inputTypes.CHECK}
              name="onReleaseImport"
              helpText="On Release Import"
              isDisabled={!supportsOnReleaseImport.value}
              {...onReleaseImport}
              onChange={onInputChange}
            />
          </div>

          {
            onReleaseImport.value &&
              <div>
                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="onUpgrade"
                  helpText="On Upgrade"
                  isDisabled={!supportsOnUpgrade.value}
                  {...onUpgrade}
                  onChange={onInputChange}
                />
              </div>
          }

          <div>
            <FormInputGroup
              type={inputTypes.CHECK}
              name="onDownloadFailure"
              helpText="On Download Failure"
              isDisabled={!supportsOnDownloadFailure.value}
              {...onDownloadFailure}
              onChange={onInputChange}
            />
          </div>

          <div>
            <FormInputGroup
              type={inputTypes.CHECK}
              name="onImportFailure"
              helpText="On Import Failure"
              isDisabled={!supportsOnImportFailure.value}
              {...onImportFailure}
              onChange={onInputChange}
            />
          </div>

          <div>
            <FormInputGroup
              type={inputTypes.CHECK}
              name="onRename"
              helpText="On Rename"
              isDisabled={!supportsOnRename.value}
              {...onRename}
              onChange={onInputChange}
            />
          </div>

          <div>
            <FormInputGroup
              type={inputTypes.CHECK}
              name="onTrackRetag"
              helpText="On Track Retag"
              isDisabled={!supportsOnTrackRetag.value}
              {...onTrackRetag}
              onChange={onInputChange}
            />
          </div>

          <div>
            <FormInputGroup
              type={inputTypes.CHECK}
              name="onHealthIssue"
              helpText="On Health Issue"
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
                  helpText="Include Health Warnings"
                  isDisabled={!supportsOnHealthIssue.value}
                  {...includeHealthWarnings}
                  onChange={onInputChange}
                />
              </div>
          }
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
