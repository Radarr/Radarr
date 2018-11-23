import PropTypes from 'prop-types';
import React from 'react';
import Modal from 'Components/Modal/Modal';
import MovieIndexOverviewOptionsModalContentConnector from './MovieIndexOverviewOptionsModalContentConnector';

function MovieIndexOverviewOptionsModal({ isOpen, onModalClose, ...otherProps }) {
  return (
    <Modal
      isOpen={isOpen}
      onModalClose={onModalClose}
    >
      <MovieIndexOverviewOptionsModalContentConnector
        {...otherProps}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

MovieIndexOverviewOptionsModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default MovieIndexOverviewOptionsModal;
