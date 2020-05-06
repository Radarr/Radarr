import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Button from 'Components/Link/Button';
import ModalContent from 'Components/Modal/ModalContent';
import ModalHeader from 'Components/Modal/ModalHeader';
import ModalBody from 'Components/Modal/ModalBody';
import ModalFooter from 'Components/Modal/ModalFooter';
import ArtistHistoryTableContent from './ArtistHistoryTableContent';

class ArtistHistoryModalContent extends Component {

  //
  // Render

  render() {
    const {
      onModalClose
    } = this.props;

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          History
        </ModalHeader>

        <ModalBody>
          <ArtistHistoryTableContent
            {...this.props}
          />
        </ModalBody>

        <ModalFooter>
          <Button onPress={onModalClose}>
            Close
          </Button>
        </ModalFooter>
      </ModalContent>
    );
  }
}

ArtistHistoryModalContent.propTypes = {
  onModalClose: PropTypes.func.isRequired
};

export default ArtistHistoryModalContent;
