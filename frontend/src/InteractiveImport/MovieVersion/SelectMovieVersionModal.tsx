import React from 'react';
import Modal from 'Components/Modal/Modal';
import SelectMovieVersionModalContent, {
  SelectMovieVersionModalContentProps,
} from './SelectMovieVersionModalContent';

interface SelectMovieVersionModalProps
  extends SelectMovieVersionModalContentProps {
  isOpen: boolean;
}

function SelectMovieVersionModal({
  isOpen,
  ...otherProps
}: SelectMovieVersionModalProps) {
  return (
    <Modal isOpen={isOpen} onModalClose={otherProps.onModalClose}>
      <SelectMovieVersionModalContent {...otherProps} />
    </Modal>
  );
}

export default SelectMovieVersionModal;
