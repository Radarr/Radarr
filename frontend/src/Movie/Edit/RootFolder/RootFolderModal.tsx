import React from 'react';
import Modal from 'Components/Modal/Modal';
import RootFolderModalContent, {
  RootFolderModalContentProps,
} from './RootFolderModalContent';

interface RootFolderModalProps extends RootFolderModalContentProps {
  isOpen: boolean;
}

function RootFolderModal({
  isOpen,
  rootFolderPath,
  movieId,
  onSavePress,
  onModalClose,
}: RootFolderModalProps) {
  return (
    <Modal isOpen={isOpen} onModalClose={onModalClose}>
      <RootFolderModalContent
        movieId={movieId}
        rootFolderPath={rootFolderPath}
        onSavePress={onSavePress}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

export default RootFolderModal;
