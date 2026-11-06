import React from 'react';
import Modal from 'Components/Modal/Modal';
import AddMovieVersionModalContent, {
  AddMovieVersionModalContentProps,
} from './AddMovieVersionModalContent';

interface AddMovieVersionModalProps extends AddMovieVersionModalContentProps {
  isOpen: boolean;
}

function AddMovieVersionModal({
  isOpen,
  ...otherProps
}: AddMovieVersionModalProps) {
  return (
    <Modal isOpen={isOpen} onModalClose={otherProps.onModalClose}>
      <AddMovieVersionModalContent {...otherProps} />
    </Modal>
  );
}

export default AddMovieVersionModal;
