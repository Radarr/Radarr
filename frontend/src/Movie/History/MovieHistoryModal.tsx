import React from 'react';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import MovieHistoryModalContent, {
  MovieHistoryModalContentProps,
} from 'Movie/History/MovieHistoryModalContent';

interface MovieHistoryModalProps extends MovieHistoryModalContentProps {
  isOpen: boolean;
}

function MovieHistoryModal({
  isOpen,
  onModalClose,
  ...otherProps
}: MovieHistoryModalProps) {
  return (
    <Modal
      isOpen={isOpen}
      size={sizes.EXTRA_EXTRA_LARGE}
      onModalClose={onModalClose}
    >
      <MovieHistoryModalContent {...otherProps} onModalClose={onModalClose} />
    </Modal>
  );
}

export default MovieHistoryModal;
