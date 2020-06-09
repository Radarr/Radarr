import PropTypes from 'prop-types';
import React from 'react';
import Modal from 'Components/Modal/Modal';
import AddListMoviePosterOptionsModalContentConnector from './AddListMoviePosterOptionsModalContentConnector';

function AddListMoviePosterOptionsModal({ isOpen, onModalClose, ...otherProps }) {
  return (
    <Modal
      isOpen={isOpen}
      onModalClose={onModalClose}
    >
      <AddListMoviePosterOptionsModalContentConnector
        {...otherProps}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

AddListMoviePosterOptionsModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default AddListMoviePosterOptionsModal;
