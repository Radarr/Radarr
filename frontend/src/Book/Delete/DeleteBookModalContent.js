import PropTypes from 'prop-types';
import React, { Component } from 'react';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes, kinds } from 'Helpers/Props';
import formatBytes from 'Utilities/Number/formatBytes';
import styles from './DeleteBookModalContent.css';

class DeleteBookModalContent extends Component {

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

  onDeleteBookConfirmed = () => {
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
      bookFileCount,
      sizeOnDisk
    } = statistics;

    const deleteFiles = this.state.deleteFiles;
    const addImportListExclusion = this.state.addImportListExclusion;

    const deleteFilesLabel = `Delete ${bookFileCount} Book Files`;
    const deleteFilesHelpText = 'Delete the book files';

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
              helpText="Prevent book from being added to Readarr by Import Lists or Author Refresh"
              kind={kinds.DANGER}
              onChange={this.onAddImportListExclusionChange}
            />
          </FormGroup>

          {
            !addImportListExclusion &&
              <div className={styles.deleteFilesMessage}>
                <div>If you don't add an import list exclusion and the author has a metadata profile other than 'None' then this book may be re-added during the next author refresh.</div>
              </div>
          }

          {
            deleteFiles &&
              <div className={styles.deleteFilesMessage}>
                <div>The book's files will be deleted.</div>

                {
                  !!bookFileCount &&
                    <div>{bookFileCount} book files totaling {formatBytes(sizeOnDisk)}</div>
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
            onPress={this.onDeleteBookConfirmed}
          >
            Delete
          </Button>
        </ModalFooter>
      </ModalContent>
    );
  }
}

DeleteBookModalContent.propTypes = {
  title: PropTypes.string.isRequired,
  statistics: PropTypes.object.isRequired,
  onDeletePress: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

DeleteBookModalContent.defaultProps = {
  statistics: {
    bookFileCount: 0
  }
};

export default DeleteBookModalContent;
