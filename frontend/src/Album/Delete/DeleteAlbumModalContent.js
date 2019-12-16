import PropTypes from 'prop-types';
import React, { Component } from 'react';
import formatBytes from 'Utilities/Number/formatBytes';
import { inputTypes, kinds } from 'Helpers/Props';
import Button from 'Components/Link/Button';
import FormGroup from 'Components/Form/FormGroup';
import FormLabel from 'Components/Form/FormLabel';
import FormInputGroup from 'Components/Form/FormInputGroup';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';
import styles from './DeleteAlbumModalContent.css';

class DeleteAlbumModalContent extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      deleteFiles: false,
      addImportListExclusion: true
    };
  }

  //
  // Listeners

  onDeleteFilesChange = ({ value }) => {
    this.setState({ deleteFiles: value });
  }

  onAddImportListExclusionChange = ({ value }) => {
    this.setState({ addImportListExclusion: value });
  }

  onDeleteAlbumConfirmed = () => {
    const deleteFiles = this.state.deleteFiles;
    const addImportListExclusion = this.state.addImportListExclusion;

    this.setState({ deleteFiles: false });
    this.setState({ addImportListExclusion: false });
    this.props.onDeletePress(deleteFiles, addImportListExclusion);
  }

  //
  // Render

  render() {
    const {
      title,
      statistics,
      onModalClose
    } = this.props;

    const {
      trackFileCount,
      sizeOnDisk
    } = statistics;

    const deleteFiles = this.state.deleteFiles;
    const addImportListExclusion = this.state.addImportListExclusion;

    const deleteFilesLabel = `Delete ${trackFileCount} Track Files`;
    const deleteFilesHelpText = 'Delete the track files';

    return (
      <ModalContent
        onModalClose={onModalClose}
      >
        <ModalHeader>
          Delete - {title}
        </ModalHeader>

        <ModalBody>

          <FormGroup>
            <FormLabel>{deleteFilesLabel}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="deleteFiles"
              value={deleteFiles}
              helpText={deleteFilesHelpText}
              kind={kinds.DANGER}
              onChange={this.onDeleteFilesChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>Add List Exclusion</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="addImportListExclusion"
              value={addImportListExclusion}
              helpText="Prevent album from being added to Lidarr by Import Lists or Artist Refresh"
              kind={kinds.DANGER}
              onChange={this.onAddImportListExclusionChange}
            />
          </FormGroup>

          {
            !addImportListExclusion &&
              <div className={styles.deleteFilesMessage}>
                <div>If you don't add an import list exclusion and the artist has a metadata profile other than 'None' then this album may be re-added during the next artist refresh.</div>
              </div>
          }

          {
            deleteFiles &&
              <div className={styles.deleteFilesMessage}>
                <div>The album's files will be deleted.</div>

                {
                  !!trackFileCount &&
                    <div>{trackFileCount} track files totaling {formatBytes(sizeOnDisk)}</div>
                }
              </div>
          }

        </ModalBody>

        <ModalFooter>
          <Button onPress={onModalClose}>
            Close
          </Button>

          <Button
            kind={kinds.DANGER}
            onPress={this.onDeleteAlbumConfirmed}
          >
            Delete
          </Button>
        </ModalFooter>
      </ModalContent>
    );
  }
}

DeleteAlbumModalContent.propTypes = {
  title: PropTypes.string.isRequired,
  statistics: PropTypes.object.isRequired,
  onDeletePress: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

DeleteAlbumModalContent.defaultProps = {
  statistics: {
    trackFileCount: 0
  }
};

export default DeleteAlbumModalContent;
