import PropTypes from 'prop-types';
import React from 'react';
import Modal from 'Components/Modal/Modal';
import ArtistHistoryContentConnector from './ArtistHistoryContentConnector';
import ArtistHistoryModalContent from './ArtistHistoryModalContent';

function ArtistHistoryModal(props) {
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
      <ArtistHistoryContentConnector
        component={ArtistHistoryModalContent}
        {...otherProps}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

ArtistHistoryModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default ArtistHistoryModal;
