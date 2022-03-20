import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Button from 'Components/Link/Button';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes, kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';

class SelectReleaseGroupModalContent extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    const {
      releaseGroup
    } = props;

    this.state = {
      releaseGroup
    };
  }

  //
  // Listeners

  onReleaseGroupChange = ({ value }) => {
    this.setState({ releaseGroup: value });
  };

  onReleaseGroupSelect = () => {
    this.props.onReleaseGroupSelect(this.state);
  };

  //
  // Render

  render() {
    const {
      onModalClose
    } = this.props;

    const {
      releaseGroup
    } = this.state;

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          {translate('ManualImportSetReleaseGroup')}
        </ModalHeader>

        <ModalBody>
          <Form>
            <FormGroup>
              <FormLabel>{translate('ReleaseGroup')}</FormLabel>

              <FormInputGroup
                type={inputTypes.TEXT}
                name="releaseGroup"
                value={releaseGroup}
                onChange={this.onReleaseGroupChange}
              />
            </FormGroup>
          </Form>
        </ModalBody>

        <ModalFooter>
          <Button onPress={onModalClose}>
            {translate('Cancel')}
          </Button>

          <Button
            kind={kinds.SUCCESS}
            onPress={this.onReleaseGroupSelect}
          >
            {translate('SetReleaseGroup')}
          </Button>
        </ModalFooter>
      </ModalContent>
    );
  }
}

SelectReleaseGroupModalContent.propTypes = {
  releaseGroup: PropTypes.string.isRequired,
  onReleaseGroupSelect: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default SelectReleaseGroupModalContent;
