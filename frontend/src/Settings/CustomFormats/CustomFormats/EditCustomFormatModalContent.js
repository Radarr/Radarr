import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { icons, inputTypes, kinds } from 'Helpers/Props';
import FieldSet from 'Components/FieldSet';
import Card from 'Components/Card';
import Icon from 'Components/Icon';
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
import Specification from './Specifications/Specification';
import AddSpecificationModal from './Specifications/AddSpecificationModal';
import EditSpecificationModalConnector from './Specifications/EditSpecificationModalConnector';
import styles from './EditCustomFormatModalContent.css';

class EditCustomFormatModalContent extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      isAddSpecificationModalOpen: false,
      isEditSpecificationModalOpen: false
    };
  }

  //
  // Listeners

  onAddSpecificationPress = () => {
    this.setState({ isAddSpecificationModalOpen: true });
  }

  onAddSpecificationModalClose = ({ specificationSelected = false } = {}) => {
    this.setState({
      isAddSpecificationModalOpen: false,
      isEditSpecificationModalOpen: specificationSelected
    });
  }

  onEditSpecificationModalClose = () => {
    this.setState({ isEditSpecificationModalOpen: false });
  }

  //
  // Render

  render() {
    const {
      isFetching,
      error,
      isSaving,
      saveError,
      item,
      specificationsPopulated,
      specifications,
      onInputChange,
      onSavePress,
      onModalClose,
      onDeleteCustomFormatPress,
      onCloneSpecificationPress,
      onConfirmDeleteSpecification,
      ...otherProps
    } = this.props;

    const {
      isAddSpecificationModalOpen,
      isEditSpecificationModalOpen
    } = this.state;

    const {
      id,
      name,
      includeCustomFormatWhenRenaming
    } = item;

    return (
      <ModalContent onModalClose={onModalClose}>

        <ModalHeader>
          {id ? 'Edit Custom Format' : 'Add Custom Format'}
        </ModalHeader>

        <ModalBody>
          <div>
            {
              isFetching &&
                <LoadingIndicator />
            }

            {
              !isFetching && !!error &&
                <div>Unable to add a new custom format, please try again.</div>
            }

            {
              !isFetching && !error && specificationsPopulated &&
                <div>
                  <Form
                    {...otherProps}
                  >
                    <FormGroup>
                      <FormLabel>
                        Name
                      </FormLabel>

                      <FormInputGroup
                        type={inputTypes.TEXT}
                        name="name"
                        {...name}
                        onChange={onInputChange}
                      />
                    </FormGroup>

                    <FormGroup>
                      <FormLabel>Include Custom Format when Renaming</FormLabel>

                      <FormInputGroup
                        type={inputTypes.CHECK}
                        name="includeCustomFormatWhenRenaming"
                        helpText="Include in {Custom Formats} renaming format"
                        {...includeCustomFormatWhenRenaming}
                        onChange={onInputChange}
                      />
                    </FormGroup>
                  </Form>

                  <FieldSet legend="Conditions">
                    <div className={styles.customFormats}>
                      {
                        specifications.map((tag) => {
                          return (
                            <Specification
                              key={tag.id}
                              {...tag}
                              onCloneSpecificationPress={onCloneSpecificationPress}
                              onConfirmDeleteSpecification={onConfirmDeleteSpecification}
                            />
                          );
                        })
                      }

                      <Card
                        className={styles.addSpecification}
                        onPress={this.onAddSpecificationPress}
                      >
                        <div className={styles.center}>
                          <Icon
                            name={icons.ADD}
                            size={45}
                          />
                        </div>
                      </Card>
                    </div>
                  </FieldSet>

                  <AddSpecificationModal
                    isOpen={isAddSpecificationModalOpen}
                    onModalClose={this.onAddSpecificationModalClose}
                  />

                  <EditSpecificationModalConnector
                    isOpen={isEditSpecificationModalOpen}
                    onModalClose={this.onEditSpecificationModalClose}
                  />
                </div>
            }
          </div>
        </ModalBody>
        <ModalFooter>
          {
            id &&
              <Button
                className={styles.deleteButton}
                kind={kinds.DANGER}
                onPress={onDeleteCustomFormatPress}
              >
                Delete
              </Button>
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
}

EditCustomFormatModalContent.propTypes = {
  isFetching: PropTypes.bool.isRequired,
  error: PropTypes.object,
  isSaving: PropTypes.bool.isRequired,
  saveError: PropTypes.object,
  item: PropTypes.object.isRequired,
  specificationsPopulated: PropTypes.bool.isRequired,
  specifications: PropTypes.arrayOf(PropTypes.object),
  onInputChange: PropTypes.func.isRequired,
  onSavePress: PropTypes.func.isRequired,
  onContentHeightChange: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired,
  onDeleteCustomFormatPress: PropTypes.func,
  onCloneSpecificationPress: PropTypes.func.isRequired,
  onConfirmDeleteSpecification: PropTypes.func.isRequired
};

export default EditCustomFormatModalContent;
