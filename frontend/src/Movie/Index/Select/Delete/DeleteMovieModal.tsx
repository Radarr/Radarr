import React from 'react';
import Modal from 'Components/Modal/Modal';
import DeleteMovieModalContent from './DeleteMovieModalContent';

interface DeleteMovieModalProps {
  isOpen: boolean;
  movieIds: number[];
  onModalClose(): void;
}

function DeleteMovieModal(props: DeleteMovieModalProps) {
  const { isOpen, movieIds, onModalClose } = props;

  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <DeleteMovieModalContent
        movieIds={movieIds}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default DeleteMovieModal;
