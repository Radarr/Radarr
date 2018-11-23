import PropTypes from 'prop-types';
import React from 'react';
import Modal from 'Components/Modal/Modal';
import MovieHistoryModalContentConnector from './MovieHistoryModalContentConnector';

function MovieHistoryModal(props) {
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
      <MovieHistoryModalContentConnector
        {...otherProps}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

MovieHistoryModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default MovieHistoryModal;
