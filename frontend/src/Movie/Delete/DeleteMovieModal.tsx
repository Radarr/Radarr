import React from 'react';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import DeleteMovieModalContent, {
  DeleteMovieModalContentProps,
} from './DeleteMovieModalContent';

interface DeleteMovieModalProps extends DeleteMovieModalContentProps {
  isOpen: boolean;
}

function DeleteMovieModal({
  isOpen,
  onModalClose,
  ...otherProps
}: DeleteMovieModalProps) {
  return (
    <Modal isOpen={isOpen} size={sizes.MEDIUM} onModalClose={onModalClose}>
      <DeleteMovieModalContent {...otherProps} onModalClose={onModalClose} />
    </Modal>
  );
}

export default DeleteMovieModal;
