import PropTypes from 'prop-types';
import React from 'react';
import { sizes } from 'Helpers/Props';
import Modal from 'Components/Modal/Modal';
import EditNetImportModalContentConnector from './EditNetImportModalContentConnector';

function EditNetImportModal({ isOpen, onModalClose, ...otherProps }) {
  return (
    <Modal
      size={sizes.MEDIUM}
      isOpen={isOpen}
      onModalClose={onModalClose}
    >
      <EditNetImportModalContentConnector
        {...otherProps}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

EditNetImportModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default EditNetImportModal;
