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
import styles from './EditImportListExclusionModalContent.css';

function EditImportListExclusionModalContent(props) {
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
    onDeleteImportExclusionPress,
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
        {id ? translate('EditListExclusion') : translate('AddListExclusion')}
      </ModalHeader>

      <ModalBody className={styles.body}>
        {
          isFetching &&
            <LoadingIndicator />
        }

        {
          !isFetching && !!error &&
            <div>
              {translate('UnableToAddANewListExclusionPleaseTryAgain')}
            </div>
        }

        {
          !isFetching && !error &&
            <Form
              {...otherProps}
            >
              <FormGroup>
                <FormLabel>{translate('TMDBId')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.NUMBER}
                  name="tmdbId"
                  helpText={translate('TmdbIdHelpText')}
                  {...tmdbId}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('MovieTitle')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.TEXT}
                  name="movieTitle"
                  helpText={translate('MovieTitleHelpText')}
                  {...movieTitle}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>{translate('MovieYear')}</FormLabel>

                <FormInputGroup
                  type={inputTypes.NUMBER}
                  name="movieYear"
                  helpText={translate('MovieYearHelpText')}
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
              onPress={onDeleteImportExclusionPress}
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

EditImportListExclusionModalContent.propTypes = {
  id: PropTypes.number,
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  item: PropTypes.object.isRequired,
  onInputChange: PropTypes.func.isRequired,
  onSavePress: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired,
  onDeleteImportExclusionPress: PropTypes.func
};

export default EditImportListExclusionModalContent;
