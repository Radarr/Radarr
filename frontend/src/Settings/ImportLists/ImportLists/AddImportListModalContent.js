import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Alert from 'Components/Alert';
import FieldSet from 'Components/FieldSet';
import Button from 'Components/Link/Button';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { kinds } from 'Helpers/Props';
import titleCase from 'Utilities/String/titleCase';
import translate from 'Utilities/String/translate';
import AddImportListItem from './AddImportListItem';
import styles from './AddImportListModalContent.css';

class AddImportListModalContent extends Component {

  //
  // Render

  render() {
    const {
      isSchemaFetching,
      isSchemaPopulated,
      schemaError,
      listGroups,
      onImportListSelect,
      onModalClose
    } = this.props;

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          {translate('AddList')}
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
                  <div>For more information on the individual importLists, clink on the info buttons.</div>
                </Alert>

                {
                  Object.keys(listGroups).map((key) => {
                    return (
                      <FieldSet legend={`${titleCase(key)} List`} key={key}>
                        <div className={styles.importLists}>
                          {
                            listGroups[key].map((importList) => {
                              return (
                                <AddImportListItem
                                  key={importList.implementation}
                                  implementation={importList.implementation}
                                  {...importList}
                                  onImportListSelect={onImportListSelect}
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
            {translate('Close')}
          </Button>
        </ModalFooter>
      </ModalContent>
    );
  }
}

AddImportListModalContent.propTypes = {
  isSchemaFetching: PropTypes.bool.isRequired,
  isSchemaPopulated: PropTypes.bool.isRequired,
  schemaError: PropTypes.object,
  listGroups: PropTypes.object.isRequired,
  onImportListSelect: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default AddImportListModalContent;
