import PropTypes from 'prop-types';
import React from 'react';
import { scrollDirections } from 'Helpers/Props';
import Button from 'Components/Link/Button';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';
import InteractiveSearchConnector from 'InteractiveSearch/InteractiveSearchConnector';

function AlbumInteractiveSearchModalContent(props) {
  const {
    bookId,
    albumTitle,
    onModalClose
  } = props;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        Interactive Search  {bookId != null && `- ${albumTitle}`}
      </ModalHeader>

      <ModalBody scrollDirection={scrollDirections.BOTH}>
        <InteractiveSearchConnector
          type="album"
          searchPayload={{
            bookId
          }}
        />
      </ModalBody>

      <ModalFooter>
        <Button onPress={onModalClose}>
          Close
        </Button>
      </ModalFooter>
    </ModalContent>
  );
}

AlbumInteractiveSearchModalContent.propTypes = {
  bookId: PropTypes.number.isRequired,
  albumTitle: PropTypes.string.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default AlbumInteractiveSearchModalContent;
