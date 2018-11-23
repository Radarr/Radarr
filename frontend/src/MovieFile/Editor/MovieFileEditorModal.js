import PropTypes from 'prop-types';
import React from 'react';
import Modal from 'Components/Modal/Modal';
import MovieFileEditorModalContentConnector from './MovieFileEditorModalContentConnector';

function MovieFileEditorModal(props) {
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
          <MovieFileEditorModalContentConnector
            {...otherProps}
            onModalClose={onModalClose}
          />
      }
    </Modal>
  );
}

MovieFileEditorModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default MovieFileEditorModal;
