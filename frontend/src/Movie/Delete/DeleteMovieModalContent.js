import PropTypes from 'prop-types';
import React, { Component } from 'react';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { icons, inputTypes, kinds } from 'Helpers/Props';
import formatBytes from 'Utilities/Number/formatBytes';
import translate from 'Utilities/String/translate';
import styles from './DeleteMovieModalContent.css';

class DeleteMovieModalContent extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      deleteFiles: false,
      addNetImportExclusion: false
    };
  }

  //
  // Listeners

  onDeleteFilesChange = ({ value }) => {
    this.setState({ deleteFiles: value });
  }

  onAddNetImportExclusionChange = ({ value }) => {
    this.setState({ addNetImportExclusion: value });
  }

  onDeleteMovieConfirmed = () => {
    const deleteFiles = this.state.deleteFiles;
    const addNetImportExclusion = this.state.addNetImportExclusion;

    this.setState({ deleteFiles: false, addNetImportExclusion: false });
    this.props.onDeletePress(deleteFiles, addNetImportExclusion);
  }

  //
  // Render

  render() {
    const {
      title,
      path,
      statistics,
      onModalClose
    } = this.props;

    const {
      movieFileCount,
      sizeOnDisk
    } = statistics;

    const deleteFiles = this.state.deleteFiles;
    const addNetImportExclusion = this.state.addNetImportExclusion;

    let deleteFilesLabel = `Delete ${movieFileCount} Movie Files`;
    let deleteFilesHelpText = 'Delete the movie files and movie folder';

    if (movieFileCount === 0) {
      deleteFilesLabel = 'Delete Movie Folder';
      deleteFilesHelpText = 'Delete the movie folder and it\'s contents';
    }

    return (
      <ModalContent
        onModalClose={onModalClose}
      >
        <ModalHeader>
          Delete - {title}
        </ModalHeader>

        <ModalBody>
          <div className={styles.pathContainer}>
            <Icon
              className={styles.pathIcon}
              name={icons.FOLDER}
            />

            {path}
          </div>

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

          {
            deleteFiles &&
              <div className={styles.deleteFilesMessage}>
                <div>The movie folder <strong>{path}</strong> and all it's content will be deleted.</div>

                {
                  !!movieFileCount &&
                    <div>{movieFileCount} movie files totaling {formatBytes(sizeOnDisk)}</div>
                }
              </div>
          }

          <FormGroup>
            <FormLabel>Add List Exclusion</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="addNetImportExclusion"
              value={addNetImportExclusion}
              helpText="Prevent movie from being added to Radarr by lists"
              kind={kinds.DANGER}
              onChange={this.onAddNetImportExclusionChange}
            />
          </FormGroup>

        </ModalBody>

        <ModalFooter>
          <Button onPress={onModalClose}>
            {translate('Close')}
          </Button>

          <Button
            kind={kinds.DANGER}
            onPress={this.onDeleteMovieConfirmed}
          >
            {translate('Delete')}
          </Button>
        </ModalFooter>
      </ModalContent>
    );
  }
}

DeleteMovieModalContent.propTypes = {
  title: PropTypes.string.isRequired,
  path: PropTypes.string.isRequired,
  statistics: PropTypes.object.isRequired,
  onDeletePress: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

DeleteMovieModalContent.defaultProps = {
  statistics: {
    movieFileCount: 0
  }
};

export default DeleteMovieModalContent;
