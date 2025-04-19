import React, { useCallback } from 'react';
import { useDispatch } from 'react-redux';
import Modal from 'Components/Modal/Modal';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import AddNewMovieCollectionMovieModalContent, {
  AddNewMovieCollectionMovieModalContentProps,
} from './AddNewMovieCollectionMovieModalContent';

interface AddNewCollectionMovieModalProps
  extends AddNewMovieCollectionMovieModalContentProps {
  isOpen: boolean;
}

function AddNewMovieCollectionMovieModal({
  isOpen,
  onModalClose,
  ...otherProps
}: AddNewCollectionMovieModalProps) {
  const dispatch = useDispatch();

  const handleModalClose = useCallback(() => {
    dispatch(clearPendingChanges({ section: 'movieCollections' }));
    onModalClose();
  }, [dispatch, onModalClose]);

  return (
    <Modal isOpen={isOpen} onModalClose={handleModalClose}>
      <AddNewMovieCollectionMovieModalContent
        {...otherProps}
        onModalClose={handleModalClose}
      />
    </Modal>
  );
}

export default AddNewMovieCollectionMovieModal;
