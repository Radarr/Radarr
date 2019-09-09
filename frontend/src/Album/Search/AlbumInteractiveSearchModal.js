import PropTypes from 'prop-types';
import React from 'react';
import Modal from 'Components/Modal/Modal';
import AlbumInteractiveSearchModalContent from './AlbumInteractiveSearchModalContent';

function AlbumInteractiveSearchModal(props) {
  const {
    isOpen,
    albumId,
    albumTitle,
    onModalClose
  } = props;

  return (
    <Modal
      isOpen={isOpen}
      closeOnBackgroundClick={false}
      onModalClose={onModalClose}
    >
      <AlbumInteractiveSearchModalContent
        albumId={albumId}
        albumTitle={albumTitle}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

AlbumInteractiveSearchModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  albumId: PropTypes.number.isRequired,
  albumTitle: PropTypes.string.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default AlbumInteractiveSearchModal;
