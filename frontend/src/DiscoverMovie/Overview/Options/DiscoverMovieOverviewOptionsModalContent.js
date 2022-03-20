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

class DiscoverMovieOverviewOptionsModalContent extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      size: props.size,
      showStudio: props.showStudio,
      showCertification: props.showCertification,
      showRatings: props.showRatings,
      showYear: props.showYear,
      showGenres: props.showGenres,
      includeRecommendations: props.includeRecommendations
    };
  }

  componentDidUpdate(prevProps) {
    const {
      size,
      showStudio,
      showYear,
      showRatings,
      showCertification,
      showGenres,
      includeRecommendations
    } = this.props;

    const state = {};

    if (size !== prevProps.size) {
      state.size = size;
    }

    if (showStudio !== prevProps.showStudio) {
      state.showStudio = showStudio;
    }

    if (showYear !== prevProps.showYear) {
      state.showYear = showYear;
    }

    if (showRatings !== prevProps.showRatings) {
      state.showRatings = showRatings;
    }

    if (showCertification !== prevProps.showCertification) {
      state.showCertification = showCertification;
    }

    if (showGenres !== prevProps.showGenres) {
      state.showGenres = showGenres;
    }

    if (includeRecommendations !== prevProps.includeRecommendations) {
      state.includeRecommendations = includeRecommendations;
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
      showStudio,
      showCertification,
      showRatings,
      showYear,
      showGenres,
      includeRecommendations
    } = this.state;

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          Overview Options
        </ModalHeader>

        <ModalBody>
          <Form>
            <FormGroup>
              <FormLabel>{translate('IncludeRadarrRecommendations')}</FormLabel>
              <FormInputGroup
                type={inputTypes.CHECK}
                name="includeRecommendations"
                value={includeRecommendations}
                helpText={translate('IncludeRecommendationsHelpText')}
                onChange={this.onChangeOption}
              />
            </FormGroup>

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
              <FormLabel>{translate('ShowGenres')}</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="showGenres"
                value={showGenres}
                onChange={this.onChangeOverviewOption}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('ShowStudio')}</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="showStudio"
                value={showStudio}
                onChange={this.onChangeOverviewOption}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('ShowYear')}</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="showYear"
                value={showYear}
                onChange={this.onChangeOverviewOption}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('ShowRatings')}</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="showRatings"
                value={showRatings}
                onChange={this.onChangeOverviewOption}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('ShowCertification')}</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="showCertification"
                value={showCertification}
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

DiscoverMovieOverviewOptionsModalContent.propTypes = {
  size: PropTypes.string.isRequired,
  showStudio: PropTypes.bool.isRequired,
  showYear: PropTypes.bool.isRequired,
  showRatings: PropTypes.bool.isRequired,
  showCertification: PropTypes.bool.isRequired,
  showGenres: PropTypes.bool.isRequired,
  includeRecommendations: PropTypes.bool.isRequired,
  onChangeOverviewOption: PropTypes.func.isRequired,
  onChangeOption: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default DiscoverMovieOverviewOptionsModalContent;
