import _ from 'lodash';
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
import { inputTypes } from 'Helpers/Props';
import translate from 'Utilities/String/translate';

const posterSizeOptions = [
  { key: 'small', value: 'Small' },
  { key: 'medium', value: 'Medium' },
  { key: 'large', value: 'Large' }
];

class MovieIndexPosterOptionsModalContent extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      detailedProgressBar: props.detailedProgressBar,
      size: props.size,
      showTitle: props.showTitle,
      showMonitored: props.showMonitored,
      showQualityProfile: props.showQualityProfile,
      showSearchAction: props.showSearchAction
    };
  }

  componentDidUpdate(prevProps) {
    const {
      detailedProgressBar,
      size,
      showTitle,
      showMonitored,
      showQualityProfile,
      showSearchAction
    } = this.props;

    const state = {};

    if (detailedProgressBar !== prevProps.detailedProgressBar) {
      state.detailedProgressBar = detailedProgressBar;
    }

    if (size !== prevProps.size) {
      state.size = size;
    }

    if (showTitle !== prevProps.showTitle) {
      state.showTitle = showTitle;
    }

    if (showMonitored !== prevProps.showMonitored) {
      state.showMonitored = showMonitored;
    }

    if (showQualityProfile !== prevProps.showQualityProfile) {
      state.showQualityProfile = showQualityProfile;
    }

    if (showSearchAction !== prevProps.showSearchAction) {
      state.showSearchAction = showSearchAction;
    }

    if (!_.isEmpty(state)) {
      this.setState(state);
    }
  }

  //
  // Listeners

  onChangePosterOption = ({ name, value }) => {
    this.setState({
      [name]: value
    }, () => {
      this.props.onChangePosterOption({ [name]: value });
    });
  }

  //
  // Render

  render() {
    const {
      onModalClose
    } = this.props;

    const {
      detailedProgressBar,
      size,
      showTitle,
      showMonitored,
      showQualityProfile,
      showSearchAction
    } = this.state;

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          {translate('PosterOptions')}
        </ModalHeader>

        <ModalBody>
          <Form>
            <FormGroup>
              <FormLabel>{translate('PosterSize')}</FormLabel>

              <FormInputGroup
                type={inputTypes.SELECT}
                name="size"
                value={size}
                values={posterSizeOptions}
                onChange={this.onChangePosterOption}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('DetailedProgressBar')}</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="detailedProgressBar"
                value={detailedProgressBar}
                helpText={translate('DetailedProgressBarHelpText')}
                onChange={this.onChangePosterOption}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('ShowTitle')}</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="showTitle"
                value={showTitle}
                helpText="Show movie title under poster"
                onChange={this.onChangePosterOption}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('ShowMonitored')}</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="showMonitored"
                value={showMonitored}
                helpText="Show monitored status under poster"
                onChange={this.onChangePosterOption}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('ShowQualityProfile')}</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="showQualityProfile"
                value={showQualityProfile}
                helpText="Show quality profile under poster"
                onChange={this.onChangePosterOption}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('ShowSearch')}</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="showSearchAction"
                value={showSearchAction}
                helpText={translate('ShowSearchHelpText')}
                onChange={this.onChangePosterOption}
              />
            </FormGroup>
          </Form>
        </ModalBody>

        <ModalFooter>
          <Button
            onPress={onModalClose}
          >
            {translate('Close')}
          </Button>
        </ModalFooter>
      </ModalContent>
    );
  }
}

MovieIndexPosterOptionsModalContent.propTypes = {
  size: PropTypes.string.isRequired,
  showTitle: PropTypes.bool.isRequired,
  showMonitored: PropTypes.bool.isRequired,
  showQualityProfile: PropTypes.bool.isRequired,
  detailedProgressBar: PropTypes.bool.isRequired,
  showSearchAction: PropTypes.bool.isRequired,
  onChangePosterOption: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default MovieIndexPosterOptionsModalContent;
