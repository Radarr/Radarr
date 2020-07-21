import PropTypes from 'prop-types';
import React from 'react';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes, kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './EditNetImportExclusionModalContent.css';

function EditNetImportExclusionModalContent(props) {
  const {
    id,
    isFetching,
    error,
    isSaving,
    saveError,
    item,
    onInputChange,
    onSavePress,
    onModalClose,
    onDeleteNetImportExclusionPress,
    ...otherProps
  } = props;

  const {
    movieTitle = '',
    tmdbId,
    movieYear
  } = item;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {id ? 'Edit List Exclusion' : 'Add List Exclusion'}
      </ModalHeader>

      <ModalBody className={styles.body}>
        {
          isFetching &&
            <LoadingIndicator />
        }

        {
          !isFetching && !!error &&
            <div>Unable to add a new list exclusion, please try again.</div>
        }

        {
          !isFetching && !error &&
            <Form
              {...otherProps}
            >
              <FormGroup>
                <FormLabel>TMDB Id</FormLabel>

                <FormInputGroup
                  type={inputTypes.NUMBER}
                  name="tmdbId"
                  helpText="The TMDB Id of the movie to exclude"
                  {...tmdbId}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>Movie Title</FormLabel>

                <FormInputGroup
                  type={inputTypes.TEXT}
                  name="movieTitle"
                  helpText="The title of the movie to exclude (can be anything meaningful)"
                  {...movieTitle}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>Movie Year</FormLabel>

                <FormInputGroup
                  type={inputTypes.NUMBER}
                  name="movieYear"
                  helpText="The year of the movie to exclude"
                  {...movieYear}
                  onChange={onInputChange}
                />
              </FormGroup>

            </Form>
        }
      </ModalBody>

      <ModalFooter>
        {
          id &&
            <Button
              className={styles.deleteButton}
              kind={kinds.DANGER}
              onPress={onDeleteNetImportExclusionPress}
            >
              {translate('Delete')}
            </Button>
        }

        <Button
          onPress={onModalClose}
        >
          {translate('Cancel')}
        </Button>

        <SpinnerErrorButton
          isSpinning={isSaving}
          error={saveError}
          onPress={onSavePress}
        >
          {translate('Save')}
        </SpinnerErrorButton>
      </ModalFooter>
    </ModalContent>
  );
}

EditNetImportExclusionModalContent.propTypes = {
  id: PropTypes.number,
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  item: PropTypes.object.isRequired,
  onInputChange: PropTypes.func.isRequired,
  onSavePress: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired,
  onDeleteNetImportExclusionPress: PropTypes.func
};

export default EditNetImportExclusionModalContent;
