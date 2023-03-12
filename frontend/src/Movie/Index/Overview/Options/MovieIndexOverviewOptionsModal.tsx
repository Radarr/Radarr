import React from 'react';
import Modal from 'Components/Modal/Modal';
import MovieIndexOverviewOptionsModalContent from './MovieIndexOverviewOptionsModalContent';

interface MovieIndexOverviewOptionsModalProps {
  isOpen: boolean;
  onModalClose(...args: unknown[]): void;
}

function MovieIndexOverviewOptionsModal({
  isOpen,
  onModalClose,
  ...otherProps
}: MovieIndexOverviewOptionsModalProps) {
  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <MovieIndexOverviewOptionsModalContent
        {...otherProps}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default MovieIndexOverviewOptionsModal;
