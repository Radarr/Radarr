import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { kinds } from 'Helpers/Props';
import Alert from 'Components/Alert';
import Button from 'Components/Link/Button';
import FieldSet from 'Components/FieldSet';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';
import AddNetImportItem from './AddNetImportItem';
import titleCase from 'Utilities/String/titleCase';
import styles from './AddNetImportModalContent.css';

class AddNetImportModalContent extends Component {

  //
  // Render

  render() {
    const {
      isSchemaFetching,
      isSchemaPopulated,
      schemaError,
      listGroups,
      onNetImportSelect,
      onModalClose
    } = this.props;

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          Add List
        </ModalHeader>

        <ModalBody>
          {
            isSchemaFetching &&
              <LoadingIndicator />
          }

          {
            !isSchemaFetching && !!schemaError &&
              <div>Unable to add a new list, please try again.</div>
          }

          {
            isSchemaPopulated && !schemaError &&
              <div>

                <Alert kind={kinds.INFO}>
                  <div>Radarr supports any RSS movie lists as well as the one stated below.</div>
                  <div>For more information on the individual netImports, clink on the info buttons.</div>
                </Alert>

                {
                  Object.keys(listGroups).map((key) => {
                    return (
                      <FieldSet legend={`${titleCase(key)} List`} key={key}>
                        <div className={styles.netImports}>
                          {
                            listGroups[key].map((netImport) => {
                              return (
                                <AddNetImportItem
                                  key={netImport.implementation}
                                  implementation={netImport.implementation}
                                  {...netImport}
                                  onNetImportSelect={onNetImportSelect}
                                />
                              );
                            })
                          }
                        </div>
                      </FieldSet>
                    );
                  })
                }
              </div>
          }
        </ModalBody>
        <ModalFooter>
          <Button
            onPress={onModalClose}
          >
            Close
          </Button>
        </ModalFooter>
      </ModalContent>
    );
  }
}

AddNetImportModalContent.propTypes = {
  isSchemaFetching: PropTypes.bool.isRequired,
  isSchemaPopulated: PropTypes.bool.isRequired,
  schemaError: PropTypes.object,
  listGroups: PropTypes.object.isRequired,
  onNetImportSelect: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default AddNetImportModalContent;
