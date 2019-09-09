import PropTypes from 'prop-types';
import React from 'react';
import Modal from 'Components/Modal/Modal';
import RetagPreviewModalContentConnector from './RetagPreviewModalContentConnector';

function RetagPreviewModal(props) {
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
      {
        isOpen &&
          <RetagPreviewModalContentConnector
            {...otherProps}
            onModalClose={onModalClose}
          />
      }
    </Modal>
  );
}

RetagPreviewModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default RetagPreviewModal;
