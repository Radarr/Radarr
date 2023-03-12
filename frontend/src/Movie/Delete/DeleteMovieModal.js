import PropTypes from 'prop-types';
import React from 'react';
import Modal from 'Components/Modal/Modal';
import { sizes } from 'Helpers/Props';
import DeleteMovieModalContentConnector from './DeleteMovieModalContentConnector';

function DeleteMovieModal(props) {
  const {
    isOpen,
    onModalClose,
    previousMovie,
    ...otherProps
  } = props;

  return (
    <Modal
      isOpen={isOpen}
      size={sizes.MEDIUM}
      onModalClose={onModalClose}
    >
      <DeleteMovieModalContentConnector
        {...otherProps}
        onModalClose={onModalClose}
        previousMovie={previousMovie}
      />
    </Modal>
  );
}

DeleteMovieModal.propTypes = {
  ...DeleteMovieModalContentConnector.propTypes,
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired,
  previousMovie: PropTypes.string
};

export default DeleteMovieModal;
