import PropTypes from 'prop-types';
import React from 'react';
import Modal from 'Components/Modal/Modal';
import RetagArtistModalContentConnector from './RetagArtistModalContentConnector';

function RetagArtistModal(props) {
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
      <RetagArtistModalContentConnector
        {...otherProps}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

RetagArtistModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default RetagArtistModal;
