import PropTypes from 'prop-types';
import React, { Component } from 'react';
import Form from 'Components/Form/Form';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import Button from 'Components/Link/Button';
import LoadingIndicator from 'Components/Loading/LoadingIndicator';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { inputTypes, kinds } from 'Helpers/Props';
import translate from 'Utilities/String/translate';

class SelectQualityModalContent extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    const {
      qualityId,
      proper,
      real
    } = props;

    this.state = {
      qualityId,
      proper,
      real
    };
  }

  //
  // Listeners

  onQualityChange = ({ value }) => {
    this.setState({ qualityId: parseInt(value) });
  };

  onProperChange = ({ value }) => {
    this.setState({ proper: value });
  };

  onRealChange = ({ value }) => {
    this.setState({ real: value });
  };

  onQualitySelect = () => {
    this.props.onQualitySelect(this.state);
  };

  //
  // Render

  render() {
    const {
      isFetching,
      isPopulated,
      error,
      items,
      onModalClose
    } = this.props;

    const {
      qualityId,
      proper,
      real
    } = this.state;

    const qualityOptions = items.map(({ id, name }) => {
      return {
        key: id,
        value: name
      };
    });

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          {translate('ManualImportSelectQuality')}
        </ModalHeader>

        <ModalBody>
          {
            isFetching &&
              <LoadingIndicator />
          }

          {
            !isFetching && !!error &&
              <div>
                {translate('UnableToLoadQualities')}
              </div>
          }

          {
            isPopulated && !error &&
              <Form>
                <FormGroup>
                  <FormLabel>{translate('Quality')}</FormLabel>

                  <FormInputGroup
                    type={inputTypes.SELECT}
                    name="quality"
                    value={qualityId}
                    values={qualityOptions}
                    onChange={this.onQualityChange}
                  />
                </FormGroup>

                <FormGroup>
                  <FormLabel>{translate('Proper')}</FormLabel>

                  <FormInputGroup
                    type={inputTypes.CHECK}
                    name="proper"
                    value={proper}
                    onChange={this.onProperChange}
                  />
                </FormGroup>

                <FormGroup>
                  <FormLabel>{translate('Real')}</FormLabel>

                  <FormInputGroup
                    type={inputTypes.CHECK}
                    name="real"
                    value={real}
                    onChange={this.onRealChange}
                  />
                </FormGroup>
              </Form>
          }
        </ModalBody>

        <ModalFooter>
          <Button onPress={onModalClose}>
            {translate('Cancel')}
          </Button>

          <Button
            kind={kinds.SUCCESS}
            onPress={this.onQualitySelect}
          >
            {translate('SelectQuality')}
          </Button>
        </ModalFooter>
      </ModalContent>
    );
  }
}

SelectQualityModalContent.propTypes = {
  qualityId: PropTypes.number.isRequired,
  proper: PropTypes.bool.isRequired,
  real: PropTypes.bool.isRequired,
  isFetching: PropTypes.bool.isRequired,
  isPopulated: PropTypes.bool.isRequired,
  error: PropTypes.object,
  items: PropTypes.arrayOf(PropTypes.object).isRequired,
  onQualitySelect: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default SelectQualityModalContent;
