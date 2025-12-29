import React from 'react';
import Modal from 'Components/Modal/Modal';
import ManageIndexersModalContent from './ManageIndexersModalContent';

interface ManageIndexersModalProps {
  isOpen: boolean;
  onModalClose(): void;
}

function ManageIndexersModal(props: Readonly<ManageIndexersModalProps>) {
  const { isOpen, onModalClose } = props;

  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <ManageIndexersModalContent onModalClose={onModalClose} />
    </Modal>
  );
}

export default ManageIndexersModal;
