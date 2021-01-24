import PropTypes from 'prop-types';
import React from 'react';
import Alert from 'Components/Alert';
import Icon from 'Components/Icon';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { icons, kinds } from 'Helpers/Props';
import styles from './OrganizeAuthorModalContent.css';

function OrganizeAuthorModalContent(props) {
  const {
    authorNames,
    onModalClose,
    onOrganizeAuthorPress
  } = props;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        Organize Selected Author
      </ModalHeader>

      <ModalBody>
        <Alert>
          Tip: To preview a rename, select "Cancel", then select any author name and use the
          <Icon
            className={styles.renameIcon}
            name={icons.ORGANIZE}
          />
        </Alert>

        <div className={styles.message}>
          Are you sure you want to organize all files in the {authorNames.length} selected author?
        </div>

        <ul>
          {
            authorNames.map((authorName) => {
              return (
                <li key={authorName}>
                  {authorName}
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
          onPress={onOrganizeAuthorPress}
        >
          Organize
        </Button>
      </ModalFooter>
    </ModalContent>
  );
}

OrganizeAuthorModalContent.propTypes = {
  authorNames: PropTypes.arrayOf(PropTypes.string).isRequired,
  onModalClose: PropTypes.func.isRequired,
  onOrganizeAuthorPress: PropTypes.func.isRequired
};

export default OrganizeAuthorModalContent;
