import PropTypes from 'prop-types';
import React from 'react';
import { inputTypes, sizes } from 'Helpers/Props';
import FieldSet from 'Components/FieldSet';
import FormGroup from 'Components/Form/FormGroup';
import FormLabel from 'Components/Form/FormLabel';
import FormInputGroup from 'Components/Form/FormInputGroup';

const branchValues = [
  'develop',
  'nightly'
];

function UpdateSettings(props) {
  const {
    advancedSettings,
    settings,
    isMono,
    isDocker,
    onInputChange
  } = props;

  const {
    branch,
    updateAutomatically,
    updateMechanism,
    updateScriptPath
  } = settings;

  if (!advancedSettings) {
    return null;
  }

  const updateOptions = [
    { key: 'builtIn', value: 'Built-In' },
    { key: 'script', value: 'Script' }
  ];

  if (isDocker) {
    return (
      <FieldSet legend="Updates">
        <div>Updating is disabled inside a docker container.  Update the container image instead.</div>
      </FieldSet>
    );
  }

  return (
    <FieldSet legend="Updates">
      <FormGroup
        advancedSettings={advancedSettings}
        isAdvanced={true}
      >
        <FormLabel>Branch</FormLabel>

        <FormInputGroup
          type={inputTypes.AUTO_COMPLETE}
          name="branch"
          helpText="Branch to use to update Lidarr"
          helpLink="https://github.com/Lidarr/Lidarr/wiki/Release-Branches"
          {...branch}
          values={branchValues}
          onChange={onInputChange}
        />
      </FormGroup>

      {
        isMono &&
        <div>
          <FormGroup
            advancedSettings={advancedSettings}
            isAdvanced={true}
            size={sizes.MEDIUM}
          >
            <FormLabel>Automatic</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="updateAutomatically"
              helpText="Automatically download and install updates. You will still be able to install from System: Updates"
              onChange={onInputChange}
              {...updateAutomatically}
            />
          </FormGroup>

          <FormGroup
            advancedSettings={advancedSettings}
            isAdvanced={true}
          >
            <FormLabel>Mechanism</FormLabel>

            <FormInputGroup
              type={inputTypes.SELECT}
              name="updateMechanism"
              values={updateOptions}
              helpText="Use Lidarr's built-in updater or a script"
              helpLink="https://github.com/Lidarr/Lidarr/wiki/Updating"
              onChange={onInputChange}
              {...updateMechanism}
            />
          </FormGroup>

          {
            updateMechanism.value === 'script' &&
            <FormGroup
              advancedSettings={advancedSettings}
              isAdvanced={true}
            >
              <FormLabel>Script Path</FormLabel>

              <FormInputGroup
                type={inputTypes.TEXT}
                name="updateScriptPath"
                helpText="Path to a custom script that takes an extracted update package and handle the remainder of the update process"
                onChange={onInputChange}
                {...updateScriptPath}
              />
            </FormGroup>
          }
        </div>
      }
    </FieldSet>
  );
}

UpdateSettings.propTypes = {
  advancedSettings: PropTypes.bool.isRequired,
  settings: PropTypes.object.isRequired,
  isMono: PropTypes.bool.isRequired,
  isDocker: PropTypes.bool.isRequired,
  onInputChange: PropTypes.func.isRequired
};

export default UpdateSettings;
