import PropTypes from 'prop-types';
import React from 'react';
import Modal from 'Components/Modal/Modal';
import AlbumInteractiveSearchModalContent from './AlbumInteractiveSearchModalContent';

function AlbumInteractiveSearchModal(props) {
  const {
    isOpen,
    bookId,
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
        bookId={bookId}
        albumTitle={albumTitle}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

AlbumInteractiveSearchModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  bookId: PropTypes.number.isRequired,
  albumTitle: PropTypes.string.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default AlbumInteractiveSearchModal;
