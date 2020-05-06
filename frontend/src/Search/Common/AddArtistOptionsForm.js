import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { icons, inputTypes, tooltipPositions } from 'Helpers/Props';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormLabel from 'Components/Form/FormLabel';
import FormInputGroup from 'Components/Form/FormInputGroup';
import Icon from 'Components/Icon';
import Popover from 'Components/Tooltip/Popover';
import ArtistMonitoringOptionsPopoverContent from 'AddArtist/ArtistMonitoringOptionsPopoverContent';
import ArtistMetadataProfilePopoverContent from 'AddArtist/ArtistMetadataProfilePopoverContent';
import styles from './AddArtistOptionsForm.css';

class AddArtistOptionsForm extends Component {

  //
  // Listeners

  onQualityProfileIdChange = ({ value }) => {
    this.props.onInputChange({ name: 'qualityProfileId', value: parseInt(value) });
  }

  onMetadataProfileIdChange = ({ value }) => {
    this.props.onInputChange({ name: 'metadataProfileId', value: parseInt(value) });
  }

  //
  // Render

  render() {
    const {
      rootFolderPath,
      monitor,
      qualityProfileId,
      metadataProfileId,
      includeNoneMetadataProfile,
      showMetadataProfile,
      tags,
      onInputChange,
      ...otherProps
    } = this.props;

    return (
      <Form {...otherProps}>
        <FormGroup>
          <FormLabel>Root Folder</FormLabel>

          <FormInputGroup
            type={inputTypes.ROOT_FOLDER_SELECT}
            name="rootFolderPath"
            onChange={onInputChange}
            {...rootFolderPath}
          />
        </FormGroup>

        <FormGroup>
          <FormLabel>
            Monitor

            <Popover
              anchor={
                <Icon
                  className={styles.labelIcon}
                  name={icons.INFO}
                />
              }
              title="Monitoring Options"
              body={<ArtistMonitoringOptionsPopoverContent />}
              position={tooltipPositions.RIGHT}
            />
          </FormLabel>

          <FormInputGroup
            type={inputTypes.MONITOR_ALBUMS_SELECT}
            name="monitor"
            onChange={onInputChange}
            {...monitor}
          />
        </FormGroup>

        <FormGroup>
          <FormLabel>Quality Profile</FormLabel>

          <FormInputGroup
            type={inputTypes.QUALITY_PROFILE_SELECT}
            name="qualityProfileId"
            onChange={this.onQualityProfileIdChange}
            {...qualityProfileId}
          />
        </FormGroup>

        <FormGroup className={showMetadataProfile ? undefined : styles.hideMetadataProfile}>
          <FormLabel>
            Metadata Profile

            {
              includeNoneMetadataProfile &&
                <Popover
                  anchor={
                    <Icon
                      className={styles.labelIcon}
                      name={icons.INFO}
                    />
                  }
                  title="Metadata Profile"
                  body={<ArtistMetadataProfilePopoverContent />}
                  position={tooltipPositions.RIGHT}
                />
            }
          </FormLabel>

          <FormInputGroup
            type={inputTypes.METADATA_PROFILE_SELECT}
            name="metadataProfileId"
            includeNone={includeNoneMetadataProfile}
            onChange={this.onMetadataProfileIdChange}
            {...metadataProfileId}
          />
        </FormGroup>

        <FormGroup>
          <FormLabel>Tags</FormLabel>

          <FormInputGroup
            type={inputTypes.TAG}
            name="tags"
            onChange={onInputChange}
            {...tags}
          />
        </FormGroup>
      </Form>
    );
  }
}

AddArtistOptionsForm.propTypes = {
  rootFolderPath: PropTypes.object,
  monitor: PropTypes.object.isRequired,
  qualityProfileId: PropTypes.object,
  metadataProfileId: PropTypes.object,
  showMetadataProfile: PropTypes.bool.isRequired,
  includeNoneMetadataProfile: PropTypes.bool.isRequired,
  tags: PropTypes.object.isRequired,
  onInputChange: PropTypes.func.isRequired
};

export default AddArtistOptionsForm;
