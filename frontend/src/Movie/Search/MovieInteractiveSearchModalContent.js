import PropTypes from 'prop-types';
import React from 'react';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { scrollDirections } from 'Helpers/Props';
import InteractiveSearchConnector from 'InteractiveSearch/InteractiveSearchConnector';

function MovieInteractiveSearchModalContent(props) {
  const {
    movieId,
    onModalClose
  } = props;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        Interactive Search
      </ModalHeader>

      <ModalBody scrollDirection={scrollDirections.BOTH}>
        <InteractiveSearchConnector
          searchPayload={{
            movieId
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

MovieInteractiveSearchModalContent.propTypes = {
  movieId: PropTypes.number.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default MovieInteractiveSearchModalContent;
