import PropTypes from 'prop-types';
import React from 'react';
import Button from 'Components/Link/Button';
import Modal from 'Components/Modal/Modal';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { sizes } from 'Helpers/Props';
import MediaInfoPopover from './Editor/MediaInfoPopover';

function FileDetailsModal(props) {
  const {
    isOpen,
    onModalClose,
    mediaInfo
  } = props;

  return (
    <Modal
      isOpen={isOpen}
      onModalClose={onModalClose}
      size={sizes.SMALL}
    >
      <ModalContent
        onModalClose={onModalClose}
      >
        <ModalHeader>
          Details
        </ModalHeader>

        <ModalBody>
          <MediaInfoPopover {...mediaInfo} />
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
  mediaInfo: PropTypes.object.isRequired
};

export default FileDetailsModal;
