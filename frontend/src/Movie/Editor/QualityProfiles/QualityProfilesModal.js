import PropTypes from 'prop-types';
import React from 'react';
import Modal from 'Components/Modal/Modal';
import QualityProfilesModalContent from './QualityProfilesModalContent';

function QualityProfilesModal(props) {
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
      <QualityProfilesModalContent
        {...otherProps}
        onModalClose={onModalClose}
      />
    </Modal>
  );
}

QualityProfilesModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default QualityProfilesModal;
