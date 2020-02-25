import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { kinds } from 'Helpers/Props';
import Alert from 'Components/Alert';
import Link from 'Components/Link/Link';
import Button from 'Components/Link/Button';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';
import AddSpecificationItem from './AddSpecificationItem';
import styles from './AddSpecificationModalContent.css';

class AddSpecificationModalContent extends Component {

  //
  // Render

  render() {
    const {
      isSchemaFetching,
      isSchemaPopulated,
      schemaError,
      schema,
      onSpecificationSelect,
      onModalClose
    } = this.props;

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          Add Condition
        </ModalHeader>

        <ModalBody>
          {
            isSchemaFetching &&
              <LoadingIndicator />
          }

          {
            !isSchemaFetching && !!schemaError &&
              <div>Unable to add a new condition, please try again.</div>
          }

          {
            isSchemaPopulated && !schemaError &&
              <div>

                <Alert kind={kinds.INFO}>
                  <div>Radarr supports custom conditions against the release properties below.</div>
                  <div>Visit <Link to='https://github.com/Radarr/Radarr/wiki/Custom-Formats-Aphrodite'>GitHub</Link> for more details.</div>
                </Alert>

                <div className={styles.specifications}>
                  {
                    schema.map((specification) => {
                      return (
                        <AddSpecificationItem
                          key={specification.implementation}
                          {...specification}
                          onSpecificationSelect={onSpecificationSelect}
                        />
                      );
                    })
                  }
                </div>

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

AddSpecificationModalContent.propTypes = {
  isSchemaFetching: PropTypes.bool.isRequired,
  isSchemaPopulated: PropTypes.bool.isRequired,
  schemaError: PropTypes.object,
  schema: PropTypes.arrayOf(PropTypes.object).isRequired,
  onSpecificationSelect: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default AddSpecificationModalContent;
