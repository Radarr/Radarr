import PropTypes from 'prop-types';
import React, { Component } from 'react';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import InlineMarkdown from 'Components/Markdown/InlineMarkdown';
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
      statistics = {},
      deleteOptions,
      onModalClose,
      onDeleteOptionChange
    } = this.props;

    const {
      movieFileCount = 0,
      sizeOnDisk = 0
    } = statistics;

    const deleteFiles = this.state.deleteFiles;
    const addImportExclusion = deleteOptions.addImportExclusion;

    return (
      <ModalContent
        onModalClose={onModalClose}
      >
        <ModalHeader>
          {translate('DeleteHeader', { title })}
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
              helpText={translate('AddListExclusionMovieHelpText')}
              kind={kinds.DANGER}
              onChange={onDeleteOptionChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{movieFileCount === 0 ? translate('DeleteMovieFolder') : translate('DeleteMovieFiles', { movieFileCount })}</FormLabel>

            <FormInputGroup
              type={inputTypes.CHECK}
              name="deleteFiles"
              value={deleteFiles}
              helpText={movieFileCount === 0 ? translate('DeleteMovieFolderHelpText') : translate('DeleteMovieFilesHelpText')}
              kind={kinds.DANGER}
              onChange={this.onDeleteFilesChange}
            />
          </FormGroup>

          {
            deleteFiles ?
              <div className={styles.deleteFilesMessage}>
                <div><InlineMarkdown data={translate('DeleteMovieFolderConfirmation', { path })} blockClassName={styles.folderPath} /></div>

                {
                  movieFileCount ?
                    <div className={styles.deleteCount}>
                      {translate('DeleteMovieFolderMovieCount', { movieFileCount, size: formatBytes(sizeOnDisk) })}
                    </div> :
                    null
                }
              </div> :
              null
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
