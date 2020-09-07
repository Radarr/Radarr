import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Card from 'Components/Card';
import FieldSet from 'Components/FieldSet';
import Icon from 'Components/Icon';
import PageSectionContent from 'Components/Page/PageSectionContent';
import { icons } from 'Helpers/Props';
import sortByName from 'Utilities/Array/sortByName';
import EditRootFolderModalConnector from './EditRootFolderModalConnector';
import RootFolder from './RootFolder';
import styles from './RootFolders.css';

class RootFolders extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isAddRootFolderModalOpen: false
    };
  }

  //
  // Listeners

  onAddRootFolderPress = () => {
    this.setState({ isAddRootFolderModalOpen: true });
  }

  onAddRootFolderModalClose = () => {
    this.setState({ isAddRootFolderModalOpen: false });
  }

  //
  // Render

  render() {
    const {
      items,
      qualityProfiles,
      metadataProfiles,
      onConfirmDeleteRootFolder,
      ...otherProps
    } = this.props;

    return (
      <FieldSet legend="Root Folders">
        <PageSectionContent
          errorMessage="Unable to load root folders"
          {...otherProps}
        >
          <div className={styles.rootFolders}>
            {
              items.sort(sortByName).map((item) => {
                const qualityProfile = qualityProfiles.find((profile) => profile.id === item.defaultQualityProfileId);
                const metadataProfile = metadataProfiles.find((profile) => profile.id === item.defaultMetadataProfileId);
                return (
                  <RootFolder
                    key={item.id}
                    {...item}
                    qualityProfile={qualityProfile}
                    metadataProfile={metadataProfile}
                    onConfirmDeleteRootFolder={onConfirmDeleteRootFolder}
                  />
                );
              })
            }

            <Card
              className={styles.addRootFolder}
              onPress={this.onAddRootFolderPress}
            >
              <div className={styles.center}>
                <Icon
                  name={icons.ADD}
                  size={45}
                />
              </div>
            </Card>
          </div>

          <EditRootFolderModalConnector
            isOpen={this.state.isAddRootFolderModalOpen}
            onModalClose={this.onAddRootFolderModalClose}
          />
        </PageSectionContent>
      </FieldSet>
    );
  }
}

RootFolders.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  qualityProfiles: PropTypes.arrayOf(PropTypes.object).isRequired,
  metadataProfiles: PropTypes.arrayOf(PropTypes.object).isRequired,
  onConfirmDeleteRootFolder: PropTypes.func.isRequired
};

export default RootFolders;
