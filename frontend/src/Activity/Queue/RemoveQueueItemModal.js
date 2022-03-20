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
import translate from 'Utilities/String/translate';

class RemoveQueueItemModal extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      remove: true,
      blocklist: false
    };
  }

  //
  // Control

  resetState = function() {
    this.setState({
      remove: true,
      blocklist: false
    });
  };

  //
  // Listeners

  onRemoveChange = ({ value }) => {
    this.setState({ remove: value });
  };

  onBlocklistChange = ({ value }) => {
    this.setState({ blocklist: value });
  };

  onRemoveConfirmed = () => {
    const state = this.state;

    this.resetState();
    this.props.onRemovePress(state);
  };

  onModalClose = () => {
    this.resetState();
    this.props.onModalClose();
  };

  //
  // Render

  render() {
    const {
      isOpen,
      sourceTitle,
      canIgnore,
      isPending
    } = this.props;

    const { remove, blocklist } = this.state;

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
            {translate('Remove')} - {sourceTitle}
          </ModalHeader>

          <ModalBody>
            <div>
              {translate('RemoveFromQueueText', [sourceTitle])}
            </div>

            {
              isPending ?
                null :
                <FormGroup>
                  <FormLabel>{translate('RemoveFromDownloadClient')}</FormLabel>

                  <FormInputGroup
                    type={inputTypes.CHECK}
                    name="remove"
                    value={remove}
                    helpTextWarning={translate('RemoveHelpTextWarning')}
                    isDisabled={!canIgnore}
                    onChange={this.onRemoveChange}
                  />
                </FormGroup>
            }

            <FormGroup>
              <FormLabel>{translate('BlocklistRelease')}</FormLabel>
              <FormInputGroup
                type={inputTypes.CHECK}
                name="blocklist"
                value={blocklist}
                helpText={translate('BlocklistHelpText')}
                onChange={this.onBlocklistChange}
              />
            </FormGroup>

          </ModalBody>

          <ModalFooter>
            <Button onPress={this.onModalClose}>
              {translate('Close')}
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
  isPending: PropTypes.bool.isRequired,
  onRemovePress: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default RemoveQueueItemModal;
