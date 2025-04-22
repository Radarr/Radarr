import React, { useCallback, useEffect } from 'react';
import { useDispatch } from 'react-redux';
import Modal from 'Components/Modal/Modal';
import usePrevious from 'Helpers/Hooks/usePrevious';
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

  const wasOpen = usePrevious(isOpen);

  const handleModalClose = useCallback(() => {
    dispatch(clearPendingChanges({ section: 'movieCollections' }));
    onModalClose();
  }, [dispatch, onModalClose]);

  useEffect(() => {
    if (wasOpen && !isOpen) {
      dispatch(clearPendingChanges({ section: 'movieCollections' }));
    }
  }, [wasOpen, isOpen, dispatch]);

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
