import React from 'react';
import Modal from 'Components/Modal/Modal';
import CalendarOptionsModalContent from './CalendarOptionsModalContent';

interface CalendarOptionsModalProps {
  isOpen: boolean;
  onModalClose: () => void;
}

function CalendarOptionsModal({
  isOpen,
  onModalClose,
}: Readonly<CalendarOptionsModalProps>) {
  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <CalendarOptionsModalContent onModalClose={onModalClose} />
    </Modal>
  );
}

export default CalendarOptionsModal;
