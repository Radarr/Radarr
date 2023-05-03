import React from 'react';
import Modal from 'Components/Modal/Modal';
import Movie from 'Movie/Movie';
import SelectMovieModalContent from './SelectMovieModalContent';

interface SelectMovieModalProps {
  isOpen: boolean;
  modalTitle: string;
  onMovieSelect(movie: Movie): void;
  onModalClose(): void;
}

function SelectMovieModal(props: SelectMovieModalProps) {
  const { isOpen, modalTitle, onMovieSelect, onModalClose } = props;

  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <SelectMovieModalContent
        modalTitle={modalTitle}
        onMovieSelect={onMovieSelect}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default SelectMovieModal;
