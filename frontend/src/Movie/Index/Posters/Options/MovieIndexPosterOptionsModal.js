import PropTypes from 'prop-types';
import React from 'react';
import Modal from 'Components/Modal/Modal';
import MovieIndexPosterOptionsModalContentConnector from './MovieIndexPosterOptionsModalContentConnector';

function MovieIndexPosterOptionsModal({ isOpen, onModalClose, ...otherProps }) {
  return (
    <Modal
      isOpen={isOpen}
      onModalClose={onModalClose}
    >
      <MovieIndexPosterOptionsModalContentConnector
        {...otherProps}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

MovieIndexPosterOptionsModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default MovieIndexPosterOptionsModal;
