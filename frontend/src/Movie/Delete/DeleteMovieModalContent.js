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
      deleteFiles: false
    };
  }

  //
  // Listeners

  onDeleteFilesChange = ({ value }) => {
    this.setState({ deleteFiles: value });
  };

  onDeleteMovieConfirmed = () => {
    const deleteFiles = this.state.deleteFiles;
    const addImportExclusion = this.props.deleteOptions.addImportExclusion;

    this.setState({ deleteFiles: false });
    this.props.onDeletePress(deleteFiles, addImportExclusion);
  };

  //
  // Render

  render() {
    const {
      title,
      path,
      hasFile,
      statistics,
      deleteOptions,
      onModalClose,
      onDeleteOptionChange
    } = this.props;

    const {
      sizeOnDisk = 0
    } = statistics;

    const deleteFiles = this.state.deleteFiles;
    const addImportExclusion = deleteOptions.addImportExclusion;

    let deleteFilesLabel = hasFile ? translate('DeleteFileLabel', [1]) : translate('DeleteFilesLabel', [0]);
    let deleteFilesHelpText = translate('DeleteFilesHelpText');

    if (!hasFile) {
      deleteFilesLabel = translate('DeleteMovieFolderLabel');
      deleteFilesHelpText = translate('DeleteMovieFolderHelpText');
    }

    return (
      <ModalContent
        onModalClose={onModalClose}
      >
        <ModalHeader>
          {translate('DeleteHeader', [title])}
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
            <FormLabel>
              {translate('AddListExclusion')}
            </FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="addImportExclusion"
              value={addImportExclusion}
              helpText={translate('AddImportExclusionHelpText')}
              kind={kinds.DANGER}
              onChange={onDeleteOptionChange}
            />
          </FormGroup>

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
                <div>
                  {translate('DeleteTheMovieFolder', { path })}
                </div>

                {
                  !!hasFile &&
                    <div>
                      {hasFile} {translate('MovieFilesTotaling')} {formatBytes(sizeOnDisk)}
                    </div>
                }
              </div>
          }

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
  hasFile: PropTypes.bool.isRequired,
  deleteOptions: PropTypes.object.isRequired,
  onDeleteOptionChange: PropTypes.func.isRequired,
  onDeletePress: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

DeleteMovieModalContent.defaultProps = {
  statistics: {}
};

export default DeleteMovieModalContent;
