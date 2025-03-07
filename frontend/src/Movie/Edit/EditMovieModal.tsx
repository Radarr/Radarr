import React, { useCallback } from 'react';
import { useDispatch } from 'react-redux';
import Modal from 'Components/Modal/Modal';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import EditMovieModalContent, {
  EditMovieModalContentProps,
} from './EditMovieModalContent';

interface EditMovieModalProps extends EditMovieModalContentProps {
  isOpen: boolean;
}

function EditMovieModal({
  isOpen,
  onModalClose,
  ...otherProps
}: EditMovieModalProps) {
  const dispatch = useDispatch();

  const handleModalClose = useCallback(() => {
    dispatch(clearPendingChanges({ section: 'movies' }));
    onModalClose();
  }, [dispatch, onModalClose]);

  return (
    <Modal isOpen={isOpen} onModalClose={handleModalClose}>
      <EditMovieModalContent {...otherProps} onModalClose={handleModalClose} />
    </Modal>
  );
}

export default EditMovieModal;
