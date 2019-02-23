import PropTypes from 'prop-types';
import React from 'react';
import Button from 'Components/Link/Button';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';
import InteractiveSearchConnector from 'InteractiveSearch/InteractiveSearchConnector';

function ArtistInteractiveSearchModalContent(props) {
  const {
    artistId,
    onModalClose
  } = props;

  return (
    <ModalContent onModalClose={onModalClose}>
      <ModalHeader>
        Interactive Search
      </ModalHeader>

      <ModalBody>
        <InteractiveSearchConnector
          type="artist"
          searchPayload={{
            artistId
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

ArtistInteractiveSearchModalContent.propTypes = {
  artistId: PropTypes.number.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default ArtistInteractiveSearchModalContent;
