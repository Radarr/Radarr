import React from 'react';
import Modal from 'Components/Modal/Modal';
import EditMoviesModalContent from './EditMoviesModalContent';

interface EditMoviesModalProps {
  isOpen: boolean;
  movieIds: number[];
  onSavePress(payload: object): void;
  onModalClose(): void;
}

function EditMoviesModal(props: EditMoviesModalProps) {
  const { isOpen, movieIds, onSavePress, onModalClose } = props;

  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <EditMoviesModalContent
        movieIds={movieIds}
        onSavePress={onSavePress}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default EditMoviesModal;
