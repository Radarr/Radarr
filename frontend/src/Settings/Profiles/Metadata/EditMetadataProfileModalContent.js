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
import PrimaryTypeItems from './PrimaryTypeItems';
import SecondaryTypeItems from './SecondaryTypeItems';
import ReleaseStatusItems from './ReleaseStatusItems';
import styles from './EditMetadataProfileModalContent.css';

function EditMetadataProfileModalContent(props) {
  const {
    isFetching,
    error,
    isSaving,
    saveError,
    primaryAlbumTypes,
    secondaryAlbumTypes,
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
    primaryAlbumTypes: itemPrimaryAlbumTypes,
    secondaryAlbumTypes: itemSecondaryAlbumTypes,
    releaseStatuses: itemReleaseStatuses
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

              <PrimaryTypeItems
                metadataProfileItems={itemPrimaryAlbumTypes.value}
                errors={itemPrimaryAlbumTypes.errors}
                warnings={itemPrimaryAlbumTypes.warnings}
                formLabel="Primary Album Types"
                {...otherProps}
              />

              <SecondaryTypeItems
                metadataProfileItems={itemSecondaryAlbumTypes.value}
                errors={itemSecondaryAlbumTypes.errors}
                warnings={itemSecondaryAlbumTypes.warnings}
                formLabel="Secondary Album Types"
                {...otherProps}
              />

              <ReleaseStatusItems
                metadataProfileItems={itemReleaseStatuses.value}
                errors={itemReleaseStatuses.errors}
                warnings={itemReleaseStatuses.warnings}
                formLabel="Release Statuses"
                {...otherProps}
              />

            </Form>
        }
      </ModalBody>
      <ModalFooter>
        {
          id &&
            <div
              className={styles.deleteButtonContainer}
              title={isInUse ? 'Can\'t delete a metadata profile that is attached to an artist or import list' : undefined}
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
  primaryAlbumTypes: PropTypes.arrayOf(PropTypes.object).isRequired,
  secondaryAlbumTypes: PropTypes.arrayOf(PropTypes.object).isRequired,
  releaseStatuses: PropTypes.arrayOf(PropTypes.object).isRequired,
  item: PropTypes.object.isRequired,
  isInUse: PropTypes.bool.isRequired,
  onInputChange: PropTypes.func.isRequired,
  onSavePress: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired,
  onDeleteMetadataProfilePress: PropTypes.func
};

export default EditMetadataProfileModalContent;
