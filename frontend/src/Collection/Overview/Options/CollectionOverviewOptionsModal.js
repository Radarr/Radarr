import PropTypes from 'prop-types';
import React from 'react';
import Modal from 'Components/Modal/Modal';
import CollectionOverviewOptionsModalContentConnector from './CollectionOverviewOptionsModalContentConnector';

function CollectionOverviewOptionsModal({ isOpen, onModalClose, ...otherProps }) {
  return (
    <Modal
      isOpen={isOpen}
      onModalClose={onModalClose}
    >
      <CollectionOverviewOptionsModalContentConnector
        {...otherProps}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

CollectionOverviewOptionsModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default CollectionOverviewOptionsModal;
