import PropTypes from 'prop-types';
import React from 'react';
import { inputTypes, kinds } from 'Helpers/Props';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormLabel from 'Components/Form/FormLabel';
import FormInputGroup from 'Components/Form/FormInputGroup';
import styles from './EditMetadataProfileModalContent.css';

function EditMetadataProfileModalContent(props) {
  const {
    isFetching,
    error,
    isSaving,
    saveError,
    item,
    isInUse,
    onInputChange,
    onSavePress,
    onModalClose,
    onDeleteMetadataProfilePress,
    ...otherProps
  } = props;

  const {
    id,
    name,
    minPopularity,
    skipMissingDate,
    skipMissingIsbn,
    skipPartsAndSets,
    skipSeriesSecondary,
    allowedLanguages
  } = item;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {id ? 'Edit Metadata Profile' : 'Add Metadata Profile'}
      </ModalHeader>

      <ModalBody>
        {
          isFetching &&
            <LoadingIndicator />
        }

        {
          !isFetching && !!error &&
            <div>Unable to add a new metadata profile, please try again.</div>
        }

        {
          !isFetching && !error &&
            <Form {...otherProps}>
              <FormGroup>
                <FormLabel>Name</FormLabel>

                <FormInputGroup
                  type={inputTypes.TEXT}
                  name="name"
                  {...name}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>Minimum Popularity</FormLabel>

                <FormInputGroup
                  type={inputTypes.NUMBER}
                  name="minPopularity"
                  {...minPopularity}
                  helpText="Popularity is average rating * number of votes"
                  isFloat={true}
                  min={0}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>Skip books with missing release date</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="skipMissingDate"
                  {...skipMissingDate}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>Skip books with no ISBN or ASIN</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="skipMissingIsbn"
                  {...skipMissingIsbn}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>Skip part books and sets</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="skipPartsAndSets"
                  {...skipPartsAndSets}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>Skip secondary series books</FormLabel>

                <FormInputGroup
                  type={inputTypes.CHECK}
                  name="skipSeriesSecondary"
                  {...skipSeriesSecondary}
                  onChange={onInputChange}
                />
              </FormGroup>

              <FormGroup>
                <FormLabel>Allowed Languages</FormLabel>

                <FormInputGroup
                  type={inputTypes.TEXT}
                  name="allowedLanguages"
                  {...allowedLanguages}
                  onChange={onInputChange}
                />
              </FormGroup>

            </Form>
        }
      </ModalBody>
      <ModalFooter>
        {
          id &&
            <div
              className={styles.deleteButtonContainer}
              title={isInUse ? 'Can\'t delete a metadata profile that is attached to an author or import list' : undefined}
            >
              <Button
                kind={kinds.DANGER}
                isDisabled={isInUse}
                onPress={onDeleteMetadataProfilePress}
              >
                Delete
              </Button>
            </div>
        }

        <Button
          onPress={onModalClose}
        >
          Cancel
        </Button>

        <SpinnerErrorButton
          isSpinning={isSaving}
          error={saveError}
          onPress={onSavePress}
        >
          Save
        </SpinnerErrorButton>
      </ModalFooter>
    </ModalContent>
  );
}

EditMetadataProfileModalContent.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  item: PropTypes.object.isRequired,
  isInUse: PropTypes.bool.isRequired,
  onInputChange: PropTypes.func.isRequired,
  onSavePress: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired,
  onDeleteMetadataProfilePress: PropTypes.func
};

export default EditMetadataProfileModalContent;
