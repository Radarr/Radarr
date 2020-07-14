import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { inputTypes, kinds } from 'Helpers/Props';
import FormGroup from 'Components/Form/FormGroup';
import FormLabel from 'Components/Form/FormLabel';
import FormInputGroup from 'Components/Form/FormInputGroup';
import Button from 'Components/Link/Button';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';
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
    this.props.onDeleteSelectedPress(deleteFiles, addNetImportExclusion);
  }

  //
  // Render

  render() {
    const {
      movies,
      onModalClose
    } = this.props;

    const deleteFiles = this.state.deleteFiles;
    const addNetImportExclusion = this.state.addNetImportExclusion;

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          Delete Selected Movie(s)
        </ModalHeader>

        <ModalBody>
          <div>
            <FormGroup>
              <FormLabel>{`Delete Movie Folder${movies.length > 1 ? 's' : ''}`}</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="deleteFiles"
                value={deleteFiles}
                helpText={`Delete Movie Folder${movies.length > 1 ? 's' : ''} and all contents`}
                kind={kinds.DANGER}
                onChange={this.onDeleteFilesChange}
              />
            </FormGroup>

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
          </div>

          <div className={styles.message}>
            {`Are you sure you want to delete ${movies.length} selected movie(s)${deleteFiles ? ' and all contents' : ''}?`}
          </div>

          <ul>
            {
              movies.map((s) => {
                return (
                  <li key={s.title}>
                    <span>{s.title}</span>

                    {
                      deleteFiles &&
                        <span className={styles.pathContainer}>
                          -
                          <span className={styles.path}>
                            {s.path}
                          </span>
                        </span>
                    }
                  </li>
                );
              })
            }
          </ul>
        </ModalBody>

        <ModalFooter>
          <Button onPress={onModalClose}>
            Cancel
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
  movies: PropTypes.arrayOf(PropTypes.object).isRequired,
  onModalClose: PropTypes.func.isRequired,
  onDeleteSelectedPress: PropTypes.func.isRequired
};

export default DeleteMovieModalContent;
