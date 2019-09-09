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
import styles from './RetagArtistModalContent.css';

function RetagArtistModalContent(props) {
  const {
    artistNames,
    onModalClose,
    onRetagArtistPress
  } = props;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        Retag Selected Artist
      </ModalHeader>

      <ModalBody>
        <Alert>
          Tip: To preview the tags that will be written... select "Cancel" then click any artist name and use the
          <Icon
            className={styles.retagIcon}
            name={icons.RETAG}
          />
        </Alert>

        <div className={styles.message}>
          Are you sure you want to re-tag all files in the {artistNames.length} selected artist?
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
          onPress={onRetagArtistPress}
        >
          Retag
        </Button>
      </ModalFooter>
    </ModalContent>
  );
}

RetagArtistModalContent.propTypes = {
  artistNames: PropTypes.arrayOf(PropTypes.string).isRequired,
  onModalClose: PropTypes.func.isRequired,
  onRetagArtistPress: PropTypes.func.isRequired
};

export default RetagArtistModalContent;
