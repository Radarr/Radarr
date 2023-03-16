import PropTypes from 'prop-types';
import React from 'react';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Button from 'Components/Link/Button';
import SpinnerErrorButton from 'Components/Link/SpinnerErrorButton';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes, kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './EditRestrictionModalContent.css';

function EditRestrictionModalContent(props) {
  const {
    isSaving,
    saveError,
    item,
    onInputChange,
    onModalClose,
    onSavePress,
    onDeleteRestrictionPress,
    ...otherProps
  } = props;

  const {
    id,
    required,
    ignored,
    tags
  } = item;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        {id ? translate('EditRestriction') : translate('AddRestriction')}
      </ModalHeader>

      <ModalBody>
        <Form
          {...otherProps}
        >
          <FormGroup>
            <FormLabel>{translate('MustContain')}</FormLabel>

            <FormInputGroup
              {...required}
              inputClassName={styles.tagInternalInput}
              type={inputTypes.TEXT_TAG}
              name="required"
              helpText={translate('RequiredRestrictionHelpText')}
              kind={kinds.SUCCESS}
              placeholder={translate('RequiredRestrictionPlaceHolder')}
              canEdit={true}
              onChange={onInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('MustNotContain')}</FormLabel>

            <FormInputGroup
              {...ignored}
              inputClassName={styles.tagInternalInput}
              type={inputTypes.TEXT_TAG}
              name="ignored"
              helpText={translate('IgnoredHelpText')}
              kind={kinds.DANGER}
              placeholder={translate('IgnoredPlaceHolder')}
              canEdit={true}
              onChange={onInputChange}
            />
          </FormGroup>

          <FormGroup>
            <FormLabel>{translate('Tags')}</FormLabel>

            <FormInputGroup
              type={inputTypes.TAG}
              name="tags"
              helpText={translate('TagsHelpText')}
              {...tags}
              onChange={onInputChange}
            />
          </FormGroup>
        </Form>
      </ModalBody>
      <ModalFooter>
        {
          id &&
            <Button
              className={styles.deleteButton}
              kind={kinds.DANGER}
              onPress={onDeleteRestrictionPress}
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

EditRestrictionModalContent.propTypes = {
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  item: PropTypes.object.isRequired,
  onInputChange: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired,
  onSavePress: PropTypes.func.isRequired,
  onDeleteRestrictionPress: PropTypes.func
};

export default EditRestrictionModalContent;
