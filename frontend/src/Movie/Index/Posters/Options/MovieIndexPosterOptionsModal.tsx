import React from 'react';
import Modal from 'Components/Modal/Modal';
import MovieIndexPosterOptionsModalContent from './MovieIndexPosterOptionsModalContent';

interface MovieIndexPosterOptionsModalProps {
  isOpen: boolean;
  onModalClose(...args: unknown[]): unknown;
}

function MovieIndexPosterOptionsModal({
  isOpen,
  onModalClose,
}: MovieIndexPosterOptionsModalProps) {
  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <MovieIndexPosterOptionsModalContent onModalClose={onModalClose} />
    </Modal>
  );
}

export default MovieIndexPosterOptionsModal;
