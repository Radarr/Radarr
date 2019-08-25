import PropTypes from 'prop-types';
import React from 'react';
import FileDetailsConnector from './FileDetailsConnector';
import Button from 'Components/Link/Button';
import Modal from 'Components/Modal/Modal';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';

function FileDetailsModal(props) {
  const {
    isOpen,
    onModalClose,
    id
  } = props;

  return (
    <Modal
      isOpen={isOpen}
      onModalClose={onModalClose}
    >
      <ModalContent
        onModalClose={onModalClose}
      >
        <ModalHeader>
          Details
        </ModalHeader>

        <ModalBody>
          <FileDetailsConnector
            id={id}
          />
        </ModalBody>

        <ModalFooter>
          <Button onPress={onModalClose}>
            Close
          </Button>
        </ModalFooter>
      </ModalContent>
    </Modal>
  );
}

FileDetailsModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired,
  id: PropTypes.number.isRequired
};

export default FileDetailsModal;
