import PropTypes from 'prop-types';
import React from 'react';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { scrollDirections } from 'Helpers/Props';
import InteractiveSearchConnector from 'InteractiveSearch/InteractiveSearchConnector';

function BookInteractiveSearchModalContent(props) {
  const {
    bookId,
    bookTitle,
    onModalClose
  } = props;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        Interactive Search  {bookId != null && `- ${bookTitle}`}
      </ModalHeader>

      <ModalBody scrollDirection={scrollDirections.BOTH}>
        <InteractiveSearchConnector
          type="book"
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

BookInteractiveSearchModalContent.propTypes = {
  bookId: PropTypes.number.isRequired,
  bookTitle: PropTypes.string.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default BookInteractiveSearchModalContent;
