import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { icons, kinds } from 'Helpers/Props';
import Card from 'Components/Card';
import IconButton from 'Components/Link/IconButton';
import ConfirmModal from 'Components/Modal/ConfirmModal';
import EditMetadataProfileModalConnector from './EditMetadataProfileModalConnector';
import styles from './MetadataProfile.css';

class MetadataProfile extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isEditMetadataProfileModalOpen: false,
      isDeleteMetadataProfileModalOpen: false
    };
  }

  //
  // Listeners

  onEditMetadataProfilePress = () => {
    this.setState({ isEditMetadataProfileModalOpen: true });
  }

  onEditMetadataProfileModalClose = () => {
    this.setState({ isEditMetadataProfileModalOpen: false });
  }

  onDeleteMetadataProfilePress = () => {
    this.setState({
      isEditMetadataProfileModalOpen: false,
      isDeleteMetadataProfileModalOpen: true
    });
  }

  onDeleteMetadataProfileModalClose = () => {
    this.setState({ isDeleteMetadataProfileModalOpen: false });
  }

  onConfirmDeleteMetadataProfile = () => {
    this.props.onConfirmDeleteMetadataProfile(this.props.id);
  }

  onCloneMetadataProfilePress = () => {
    const {
      id,
      onCloneMetadataProfilePress
    } = this.props;

    onCloneMetadataProfilePress(id);
  }

  //
  // Render

  render() {
    const {
      id,
      name,
      isDeleting
    } = this.props;

    return (
      <Card
        className={styles.metadataProfile}
        overlayContent={true}
        onPress={this.onEditMetadataProfilePress}
      >
        <div className={styles.nameContainer}>
          <div className={styles.name}>
            {name}
          </div>

          <IconButton
            className={styles.cloneButton}
            title="Clone Profile"
            name={icons.CLONE}
            onPress={this.onCloneMetadataProfilePress}
          />
        </div>

        <EditMetadataProfileModalConnector
          id={id}
          isOpen={this.state.isEditMetadataProfileModalOpen}
          onModalClose={this.onEditMetadataProfileModalClose}
          onDeleteMetadataProfilePress={this.onDeleteMetadataProfilePress}
        />

        <ConfirmModal
          isOpen={this.state.isDeleteMetadataProfileModalOpen}
          kind={kinds.DANGER}
          title="Delete Metadata Profile"
          message={`Are you sure you want to delete the metadata profile '${name}'?`}
          confirmLabel="Delete"
          isSpinning={isDeleting}
          onConfirm={this.onConfirmDeleteMetadataProfile}
          onCancel={this.onDeleteMetadataProfileModalClose}
        />
      </Card>
    );
  }
}

MetadataProfile.propTypes = {
  id: PropTypes.number.isRequired,
  name: PropTypes.string.isRequired,
  isDeleting: PropTypes.bool.isRequired,
  onConfirmDeleteMetadataProfile: PropTypes.func.isRequired,
  onCloneMetadataProfilePress: PropTypes.func.isRequired

};

export default MetadataProfile;
