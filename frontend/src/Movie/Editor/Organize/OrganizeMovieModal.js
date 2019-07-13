import PropTypes from 'prop-types';
import React from 'react';
import Modal from 'Components/Modal/Modal';
import OrganizeMovieModalContentConnector from './OrganizeMovieModalContentConnector';

function OrganizeMovieModal(props) {
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
      <OrganizeMovieModalContentConnector
        {...otherProps}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

OrganizeMovieModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default OrganizeMovieModal;
