import React from 'react';
import Button from 'Components/Link/Button';
import Modal from 'Components/Modal/Modal';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { kinds, sizes } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import styles from './MoveMovieModal.css';

interface MoveMovieModalProps {
  originalPath?: string;
  destinationPath?: string;
  destinationRootFolder?: string;
  isOpen: boolean;
  onModalClose: () => void;
  onSavePress: () => void;
  onMoveMoviePress: () => void;
}

function MoveMovieModal({
  originalPath,
  destinationPath,
  destinationRootFolder,
  isOpen,
  onModalClose,
  onSavePress,
  onMoveMoviePress,
}: MoveMovieModalProps) {
  if (isOpen && !originalPath && !destinationPath && !destinationRootFolder) {
    console.error(
      'originalPath and destinationPath OR destinationRootFolder must be provided'
    );
  }

  return (
    <Modal
      isOpen={isOpen}
      size={sizes.MEDIUM}
      closeOnBackgroundClick={false}
      onModalClose={onModalClose}
    >
      <ModalContent showCloseButton={true} onModalClose={onModalClose}>
        <ModalHeader>{translate('MoveFiles')}</ModalHeader>

        <ModalBody>
          {destinationRootFolder
            ? translate('MoveMovieFoldersToRootFolder', {
                destinationRootFolder,
              })
            : null}

          {originalPath && destinationPath
            ? translate('MoveMovieFoldersToNewPath', {
                originalPath,
                destinationPath,
              })
            : null}

          {destinationRootFolder ? (
            <div className={styles.folderRenameMessage}>
              {translate('MoveMovieFoldersRenameFolderWarning')}
            </div>
          ) : null}
        </ModalBody>

        <ModalFooter>
          <Button className={styles.doNotMoveButton} onPress={onSavePress}>
            {translate('MoveMovieFoldersDontMoveFiles')}
          </Button>

          <Button kind={kinds.DANGER} onPress={onMoveMoviePress}>
            {translate('MoveMovieFoldersMoveFiles')}
          </Button>
        </ModalFooter>
      </ModalContent>
    </Modal>
  );
}

export default MoveMovieModal;
