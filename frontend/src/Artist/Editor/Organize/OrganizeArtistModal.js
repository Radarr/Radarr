import PropTypes from 'prop-types';
import React from 'react';
import Modal from 'Components/Modal/Modal';
import OrganizeArtistModalContentConnector from './OrganizeArtistModalContentConnector';

function OrganizeArtistModal(props) {
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
      <OrganizeArtistModalContentConnector
        {...otherProps}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

OrganizeArtistModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default OrganizeArtistModal;
