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
  { key: 'small', value: translate('Small') },
  { key: 'medium', value: translate('Medium') },
  { key: 'large', value: translate('Large') }
];

class CollectionOverviewOptionsModalContent extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      detailedProgressBar: props.detailedProgressBar,
      size: props.size,
      showDetails: props.showDetails,
      showOverview: props.showOverview,
      showPosters: props.showPosters
    };
  }

  componentDidUpdate(prevProps) {
    const {
      detailedProgressBar,
      size,
      showDetails,
      showOverview,
      showPosters
    } = this.props;

    const state = {};

    if (detailedProgressBar !== prevProps.detailedProgressBar) {
      state.detailedProgressBar = detailedProgressBar;
    }

    if (size !== prevProps.size) {
      state.size = size;
    }

    if (showDetails !== prevProps.showDetails) {
      state.showDetails = showDetails;
    }

    if (showOverview !== prevProps.showOverview) {
      state.showOverview = showOverview;
    }

    if (showPosters !== prevProps.showPosters) {
      state.showPosters = showPosters;
    }

    if (!_.isEmpty(state)) {
      this.setState(state);
    }
  }

  //
  // Listeners

  onChangeOverviewOption = ({ name, value }) => {
    this.setState({
      [name]: value
    }, () => {
      this.props.onChangeOverviewOption({ [name]: value });
    });
  };

  onChangeOption = ({ name, value }) => {
    this.setState({
      [name]: value
    }, () => {
      this.props.onChangeOption({
        [name]: value
      });
    });
  };

  //
  // Render

  render() {
    const {
      onModalClose
    } = this.props;

    const {
      size,
      detailedProgressBar,
      showDetails,
      showPosters,
      showOverview
    } = this.state;

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          {translate('CollectionOptions')}
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
                onChange={this.onChangeOverviewOption}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('DetailedProgressBar')}</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="detailedProgressBar"
                value={detailedProgressBar}
                helpText={translate('DetailedProgressBarHelpText')}
                onChange={this.onChangeOverviewOption}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('ShowCollectionDetails')}</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="showDetails"
                value={showDetails}
                helpText={translate('CollectionShowDetailsHelpText')}
                onChange={this.onChangeOverviewOption}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('ShowOverview')}</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="showOverview"
                value={showOverview}
                helpText={translate('CollectionShowOverviewsHelpText')}
                onChange={this.onChangeOverviewOption}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('ShowPosters')}</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="showPosters"
                value={showPosters}
                helpText={translate('CollectionShowPostersHelpText')}
                onChange={this.onChangeOverviewOption}
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

CollectionOverviewOptionsModalContent.propTypes = {
  detailedProgressBar: PropTypes.bool.isRequired,
  size: PropTypes.string.isRequired,
  showDetails: PropTypes.bool.isRequired,
  showOverview: PropTypes.bool.isRequired,
  showPosters: PropTypes.bool.isRequired,
  onChangeOverviewOption: PropTypes.func.isRequired,
  onChangeOption: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default CollectionOverviewOptionsModalContent;
