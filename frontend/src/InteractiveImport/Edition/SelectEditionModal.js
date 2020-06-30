import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Modal from 'Components/Modal/Modal';
import SelectEditionModalContentConnector from './SelectEditionModalContentConnector';

class SelectEditionModal extends Component {

  //
  // Render

  render() {
    const {
      isOpen,
      onModalClose,
      ...otherProps
    } = this.props;

    return (
      <Modal
        isOpen={isOpen}
        onModalClose={onModalClose}
      >
        <SelectEditionModalContentConnector
          {...otherProps}
          onModalClose={onModalClose}
        />
      </Modal>
    );
  }
}

SelectEditionModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default SelectEditionModal;
