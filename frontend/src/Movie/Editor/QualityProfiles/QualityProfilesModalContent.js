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

class QualityProfilesModalContent extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      qualityProfileIds: []
    };
  }

  //
  // Lifecycle

  onInputChange = ({ name, value }) => {
    this.setState({ [name]: value });
  };

  onApplyQualityProfilesPress = () => {
    const {
      qualityProfileIds
    } = this.state;

    this.props.onApplyQualityProfilesPress(qualityProfileIds);
  };

  //
  // Render

  render() {
    const {
      onModalClose
    } = this.props;

    const {
      qualityProfileIds
    } = this.state;

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          {translate('QualityProfiles')}
        </ModalHeader>

        <ModalBody>
          <Form>
            <FormGroup>
              <FormLabel>{translate('QualityProfiles')}</FormLabel>

              <FormInputGroup
                type={inputTypes.QUALITY_PROFILE_SELECT}
                name="qualityProfileIds"
                value={qualityProfileIds}
                onChange={this.onInputChange}
              />
            </FormGroup>
          </Form>
        </ModalBody>

        <ModalFooter>
          <Button onPress={onModalClose}>
            {translate('Cancel')}
          </Button>

          <Button
            kind={kinds.PRIMARY}
            onPress={this.onApplyQualityProfilesPress}
          >
            {translate('Apply')}
          </Button>
        </ModalFooter>
      </ModalContent>
    );
  }
}

QualityProfilesModalContent.propTypes = {
  onModalClose: PropTypes.func.isRequired,
  onApplyQualityProfilesPress: PropTypes.func.isRequired
};

export default QualityProfilesModalContent;
