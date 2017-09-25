import PropTypes from 'prop-types';
import React from 'react';
import Modal from 'Components/Modal/Modal';
import TrackFileEditorModalContentConnector from './TrackFileEditorModalContentConnector';

function TrackFileEditorModal(props) {
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
          <TrackFileEditorModalContentConnector
            {...otherProps}
            onModalClose={onModalClose}
          />
      }
    </Modal>
  );
}

TrackFileEditorModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default TrackFileEditorModal;
