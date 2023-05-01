import PropTypes from 'prop-types';
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

function MoveMovieModal(props) {
  const {
    originalPath,
    destinationPath,
    destinationRootFolder,
    isOpen,
    onModalClose,
    onSavePress,
    onMoveMoviePress
  } = props;

  if (
    isOpen &&
    !originalPath &&
    !destinationPath &&
    !destinationRootFolder
  ) {
    console.error('orginalPath and destinationPath OR destinationRootFolder must be provided');
  }

  return (
    <Modal
      isOpen={isOpen}
      size={sizes.MEDIUM}
      closeOnBackgroundClick={false}
      onModalClose={onModalClose}
    >
      <ModalContent
        showCloseButton={true}
        onModalClose={onModalClose}
      >
        <ModalHeader>
          {translate('MoveFiles')}
        </ModalHeader>

        <ModalBody>
          {
            destinationRootFolder ?
              translate('MoveFolders1', [destinationRootFolder]) :
              translate('MoveFolders2', [originalPath, destinationPath])
          }
          {
            destinationRootFolder ?
              <div>
                {translate('FolderMoveRenameWarning')}
              </div> :
              null
          }
        </ModalBody>

        <ModalFooter>
          <Button
            className={styles.doNotMoveButton}
            onPress={onSavePress}
          >
            {translate('NoMoveFilesSelf')}
          </Button>

          <Button
            kind={kinds.DANGER}
            onPress={onMoveMoviePress}
          >
            {translate('YesMoveFiles')}
          </Button>
        </ModalFooter>
      </ModalContent>
    </Modal>
  );
}

MoveMovieModal.propTypes = {
  originalPath: PropTypes.string,
  destinationPath: PropTypes.string,
  destinationRootFolder: PropTypes.string,
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired,
  onSavePress: PropTypes.func.isRequired,
  onMoveMoviePress: PropTypes.func.isRequired
};

export default MoveMovieModal;
