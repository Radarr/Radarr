import React, { useCallback } from 'react';
import { useDispatch } from 'react-redux';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import {
  cancelFetchReleases,
  clearReleases,
} from 'Store/Actions/releaseActions';
import MovieInteractiveSearchModalContent from './MovieInteractiveSearchModalContent';

interface MovieInteractiveSearchModalProps {
  isOpen: boolean;
  movieId: number;
  movieTitle?: string;
  onModalClose(): void;
}

function MovieInteractiveSearchModal(props: MovieInteractiveSearchModalProps) {
  const { isOpen, movieId, movieTitle, onModalClose } = props;

  const dispatch = useDispatch();

  const handleModalClose = useCallback(() => {
    dispatch(cancelFetchReleases());
    dispatch(clearReleases());

    onModalClose();
  }, [dispatch, onModalClose]);

  return (
    <Modal
      isOpen={isOpen}
      closeOnBackgroundClick={false}
      size={sizes.EXTRA_EXTRA_LARGE}
      onModalClose={handleModalClose}
    >
      <MovieInteractiveSearchModalContent
        movieId={movieId}
        movieTitle={movieTitle}
        onModalClose={handleModalClose}
      />
    </Modal>
  );
}

export default MovieInteractiveSearchModal;
