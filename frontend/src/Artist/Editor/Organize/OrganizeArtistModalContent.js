import PropTypes from 'prop-types';
import React from 'react';
import { icons, kinds } from 'Helpers/Props';
import Alert from 'Components/Alert';
import Button from 'Components/Link/Button';
import Icon from 'Components/Icon';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';
import styles from './OrganizeArtistModalContent.css';

function OrganizeArtistModalContent(props) {
  const {
    artistNames,
    onModalClose,
    onOrganizeArtistPress
  } = props;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        Organize Selected Artist
      </ModalHeader>

      <ModalBody>
        <Alert>
          Tip: To preview a rename... select "Cancel" then click any artist name and use the
          <Icon
            className={styles.renameIcon}
            name={icons.ORGANIZE}
          />
        </Alert>

        <div className={styles.message}>
          Are you sure you want to organize all files in the {artistNames.length} selected artist?
        </div>

        <ul>
          {
            artistNames.map((artistName) => {
              return (
                <li key={artistName}>
                  {artistName}
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
          onPress={onOrganizeArtistPress}
        >
          Organize
        </Button>
      </ModalFooter>
    </ModalContent>
  );
}

OrganizeArtistModalContent.propTypes = {
  artistNames: PropTypes.arrayOf(PropTypes.string).isRequired,
  onModalClose: PropTypes.func.isRequired,
  onOrganizeArtistPress: PropTypes.func.isRequired
};

export default OrganizeArtistModalContent;
