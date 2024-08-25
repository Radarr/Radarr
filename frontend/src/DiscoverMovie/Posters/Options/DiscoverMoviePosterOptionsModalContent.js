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

class DiscoverMoviePosterOptionsModalContent extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      size: props.size,
      showTitle: props.showTitle,
      showTmdbRating: props.showTmdbRating,
      showImdbRating: props.showImdbRating,
      showRottenTomatoesRating: props.showRottenTomatoesRating,
      includeRecommendations: props.includeRecommendations,
      includeTrending: props.includeTrending,
      includePopular: props.includePopular
    };
  }

  componentDidUpdate(prevProps) {
    const {
      size,
      showTitle,
      showTmdbRating,
      showImdbRating,
      showRottenTomatoesRating,
      includeRecommendations,
      includeTrending,
      includePopular
    } = this.props;

    const state = {};

    if (size !== prevProps.size) {
      state.size = size;
    }

    if (showTitle !== prevProps.showTitle) {
      state.showTitle = showTitle;
    }

    if (showTmdbRating !== prevProps.showTmdbRating) {
      state.showTmdbRating = showTmdbRating;
    }

    if (showImdbRating !== prevProps.showImdbRating) {
      state.showImdbRating = showImdbRating;
    }

    if (showRottenTomatoesRating !== prevProps.showRottenTomatoesRating) {
      state.showRottenTomatoesRating = showRottenTomatoesRating;
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

  onChangePosterOption = ({ name, value }) => {
    this.setState({
      [name]: value
    }, () => {
      this.props.onChangePosterOption({ [name]: value });
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
      showTitle,
      showTmdbRating,
      showImdbRating,
      showRottenTomatoesRating,
      includeRecommendations,
      includeTrending,
      includePopular
    } = this.state;

    return (
      <ModalContent onModalClose={onModalClose}>
        <ModalHeader>
          {translate('PosterOptions')}
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
                onChange={this.onChangePosterOption}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('ShowTitle')}</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="showTitle"
                value={showTitle}
                helpText={translate('ShowTitleHelpText')}
                onChange={this.onChangePosterOption}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('ShowTmdbRating')}</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="showTmdbRating"
                value={showTmdbRating}
                helpText={translate('ShowTmdbRatingHelpText')}
                onChange={this.onChangePosterOption}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('ShowImdbRating')}</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="showImdbRating"
                value={showImdbRating}
                helpText={translate('ShowImdbRatingHelpText')}
                onChange={this.onChangePosterOption}
              />
            </FormGroup>

            <FormGroup>
              <FormLabel>{translate('ShowRottenTomatoesRating')}</FormLabel>

              <FormInputGroup
                type={inputTypes.CHECK}
                name="showRottenTomatoesRating"
                value={showRottenTomatoesRating}
                helpText={translate('ShowRottenTomatoesRatingHelpText')}
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

DiscoverMoviePosterOptionsModalContent.propTypes = {
  size: PropTypes.string.isRequired,
  showTitle: PropTypes.bool.isRequired,
  showTmdbRating: PropTypes.bool.isRequired,
  showImdbRating: PropTypes.bool.isRequired,
  showRottenTomatoesRating: PropTypes.bool.isRequired,
  includeRecommendations: PropTypes.bool.isRequired,
  includeTrending: PropTypes.bool.isRequired,
  includePopular: PropTypes.bool.isRequired,
  onChangePosterOption: PropTypes.func.isRequired,
  onChangeOption: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default DiscoverMoviePosterOptionsModalContent;
