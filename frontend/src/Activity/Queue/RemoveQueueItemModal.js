import PropTypes from 'prop-types';
import React, { Component } from 'react';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Button from 'Components/Link/Button';
import Modal from 'Components/Modal/Modal';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes, kinds, sizes } from 'Helpers/Props';

class RemoveQueueItemModal extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      remove: true,
      blacklist: false,
      skipredownload: false
    };
  }

  //
  // Control

  resetState = function() {
    this.setState({
      remove: true,
      blacklist: false,
      skipredownload: false
    });
  }

  //
  // Listeners

  onRemoveChange = ({ value }) => {
    this.setState({ remove: value });
  }

  onBlacklistChange = ({ value }) => {
    this.setState({ blacklist: value });
  }

  onSkipReDownloadChange = ({ value }) => {
    this.setState({ skipredownload: value });
  }

  onRemoveConfirmed = () => {
    const state = this.state;

    this.resetState();
    this.props.onRemovePress(state);
  }

  onModalClose = () => {
    this.resetState();
    this.props.onModalClose();
  }

  //
  // Render

  render() {
    const {
      isOpen,
      sourceTitle,
      canIgnore
    } = this.props;

    const { remove, blacklist, skipredownload } = this.state;

    return (
      <Modal
        isOpen={isOpen}
        size={sizes.MEDIUM}
        onModalClose={this.onModalClose}
      >
        <ModalContent
          onModalClose={this.onModalClose}
        >
          <ModalHeader>
            Remove - {sourceTitle}
          </ModalHeader>

          <ModalBody>
            <div>
              Are you sure you want to remove '{sourceTitle}' from the queue?
            </div>

            <FormGroup>
              <FormLabel>Remove From Download Client</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="remove"
                value={remove}
                helpTextWarning="Removing will remove the download and the file(s) from the download client."
                isDisabled={!canIgnore}
                onChange={this.onRemoveChange}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>Blacklist Release</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="blacklist"
                value={blacklist}
                helpText="Prevents Readarr from automatically grabbing this release again"
                onChange={this.onBlacklistChange}
              />
            </FormGroup>

            {
              blacklist &&
                <FormGroup>
                  <FormLabel>Skip Redownload</FormLabel>
                  <FormInputGroup
                    type={inputTypes.CHECK}
                    name="skipredownload"
                    value={skipredownload}
                    helpText="Prevents Readarr from trying download an alternative release for this item"
                    onChange={this.onSkipReDownloadChange}
                  />
                </FormGroup>
            }

          </ModalBody>

          <ModalFooter>
            <Button onPress={this.onModalClose}>
              Close
            </Button>

            <Button
              kind={kinds.DANGER}
              onPress={this.onRemoveConfirmed}
            >
              Remove
            </Button>
          </ModalFooter>
        </ModalContent>
      </Modal>
    );
  }
}

RemoveQueueItemModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  sourceTitle: PropTypes.string.isRequired,
  canIgnore: PropTypes.bool.isRequired,
  onRemovePress: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default RemoveQueueItemModal;
