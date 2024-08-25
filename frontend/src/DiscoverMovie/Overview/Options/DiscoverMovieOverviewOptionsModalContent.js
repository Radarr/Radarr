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
  {
    key: 'small',
    get value() {
      return translate('Small');
    }
  },
  {
    key: 'medium',
    get value() {
      return translate('Medium');
    }
  },
  {
    key: 'large',
    get value() {
      return translate('Large');
    }
  }
];

class DiscoverMovieOverviewOptionsModalContent extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      size: props.size,
      showYear: props.showYear,
      showStudio: props.showStudio,
      showGenres: props.showGenres,
      showTmdbRating: props.showTmdbRating,
      showImdbRating: props.showImdbRating,
      showCertification: props.showCertification,
      includeRecommendations: props.includeRecommendations,
      includeTrending: props.includeTrending,
      includePopular: props.includePopular
    };
  }

  componentDidUpdate(prevProps) {
    const {
      size,
      showYear,
      showStudio,
      showGenres,
      showTmdbRating,
      showImdbRating,
      showCertification,
      includeRecommendations,
      includeTrending,
      includePopular
    } = this.props;

    const state = {};

    if (size !== prevProps.size) {
      state.size = size;
    }

    if (showYear !== prevProps.showYear) {
      state.showYear = showYear;
    }

    if (showStudio !== prevProps.showStudio) {
      state.showStudio = showStudio;
    }

    if (showGenres !== prevProps.showGenres) {
      state.showGenres = showGenres;
    }

    if (showTmdbRating !== prevProps.showTmdbRating) {
      state.showTmdbRating = showTmdbRating;
    }

    if (showImdbRating !== prevProps.showImdbRating) {
      state.showImdbRating = showImdbRating;
    }

    if (showCertification !== prevProps.showCertification) {
      state.showCertification = showCertification;
    }

    if (includeRecommendations !== prevProps.includeRecommendations) {
      state.includeRecommendations = includeRecommendations;
    }

    if (includeTrending !== prevProps.includeTrending) {
      state.includeTrending = includeTrending;
    }

    if (includePopular !== prevProps.includePopular) {
      state.includePopular = includePopular;
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
      showYear,
      showStudio,
      showGenres,
      showTmdbRating,
      showImdbRating,
      showCertification,
      includeRecommendations,
      includeTrending,
      includePopular
    } = this.state;

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          {translate('OverviewOptions')}
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
              <FormLabel>{translate('IncludeTrending')}</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="includeTrending"
                value={includeTrending}
                helpText={translate('IncludeTrendingMoviesHelpText')}
                onChange={this.onChangeOption}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('IncludePopular')}</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="includePopular"
                value={includePopular}
                helpText={translate('IncludePopularMoviesHelpText')}
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
              <FormLabel>{translate('ShowYear')}</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="showYear"
                value={showYear}
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
              <FormLabel>{translate('ShowGenres')}</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="showGenres"
                value={showGenres}
                onChange={this.onChangeOverviewOption}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('ShowTmdbRating')}</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="showTmdbRating"
                value={showTmdbRating}
                onChange={this.onChangeOverviewOption}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('ShowImdbRating')}</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="showImdbRating"
                value={showImdbRating}
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
  showYear: PropTypes.bool.isRequired,
  showStudio: PropTypes.bool.isRequired,
  showGenres: PropTypes.bool.isRequired,
  showTmdbRating: PropTypes.bool.isRequired,
  showImdbRating: PropTypes.bool.isRequired,
  showCertification: PropTypes.bool.isRequired,
  includeRecommendations: PropTypes.bool.isRequired,
  includeTrending: PropTypes.bool.isRequired,
  includePopular: PropTypes.bool.isRequired,
  onChangeOverviewOption: PropTypes.func.isRequired,
  onChangeOption: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default DiscoverMovieOverviewOptionsModalContent;
