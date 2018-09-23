import PropTypes from 'prop-types';
import React from 'react';
import Modal from 'Components/Modal/Modal';
import InteractiveSearchModalContentConnector from './InteractiveSearchModalContentConnector';

function InteractiveSearchModal(props) {
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
      <InteractiveSearchModalContentConnector
        {...otherProps}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

InteractiveSearchModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default InteractiveSearchModal;
