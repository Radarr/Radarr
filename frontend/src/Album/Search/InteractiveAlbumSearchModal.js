import PropTypes from 'prop-types';
import React from 'react';
import Modal from 'Components/Modal/Modal';
import InteractiveAlbumSearchModalContentConnector from './InteractiveAlbumSearchModalContentConnector';

function InteractiveAlbumSearchModal(props) {
  const {
    isOpen,
    onModalClose,
    ...otherProps
  } = props;

  return (
    <Modal
      isOpen={isOpen}
      onModalClose={onModalClose}
    >
      <InteractiveAlbumSearchModalContentConnector
        {...otherProps}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

InteractiveAlbumSearchModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default InteractiveAlbumSearchModal;
