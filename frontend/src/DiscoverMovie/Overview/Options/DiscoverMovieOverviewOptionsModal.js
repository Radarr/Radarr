import PropTypes from 'prop-types';
import React from 'react';
import Modal from 'Components/Modal/Modal';
import DiscoverMovieOverviewOptionsModalContentConnector from './DiscoverMovieOverviewOptionsModalContentConnector';

function DiscoverMovieOverviewOptionsModal({ isOpen, onModalClose, ...otherProps }) {
  return (
    <Modal
      isOpen={isOpen}
      onModalClose={onModalClose}
    >
      <DiscoverMovieOverviewOptionsModalContentConnector
        {...otherProps}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

DiscoverMovieOverviewOptionsModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default DiscoverMovieOverviewOptionsModal;
