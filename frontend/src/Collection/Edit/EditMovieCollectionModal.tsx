import React, { useCallback } from 'react';
import { useDispatch } from 'react-redux';
import Modal from 'Components/Modal/Modal';
import { clearPendingChanges } from 'Store/Actions/baseActions';
import EditMovieCollectionModalContent, {
  EditMovieCollectionModalContentProps,
} from './EditMovieCollectionModalContent';

interface EditMovieCollectionModalProps
  extends EditMovieCollectionModalContentProps {
  isOpen: boolean;
}

function EditMovieCollectionModal({
  isOpen,
  onModalClose,
  ...otherProps
}: EditMovieCollectionModalProps) {
  const dispatch = useDispatch();

  const handleModalClose = useCallback(() => {
    dispatch(clearPendingChanges({ section: 'movieCollections' }));
    onModalClose();
  }, [dispatch, onModalClose]);

  return (
    <Modal isOpen={isOpen} onModalClose={handleModalClose}>
      <EditMovieCollectionModalContent
        {...otherProps}
        onModalClose={handleModalClose}
      />
    </Modal>
  );
}

export default EditMovieCollectionModal;
