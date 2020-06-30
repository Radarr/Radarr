import PropTypes from 'prop-types';
import React from 'react';
import Modal from 'Components/Modal/Modal';
import EditBookModalContentConnector from './EditBookModalContentConnector';

function EditBookModal({ isOpen, onModalClose, ...otherProps }) {
  return (
    <Modal
      isOpen={isOpen}
      onModalClose={onModalClose}
    >
      <EditBookModalContentConnector
        {...otherProps}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

EditBookModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default EditBookModal;
