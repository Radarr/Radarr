import PropTypes from 'prop-types';
import React, { Component } from 'react';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import { inputTypes } from 'Helpers/Props';
import translate from 'Utilities/String/translate';

class DiscoverMovieTableOptions extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      includeRecommendations: props.includeRecommendations,
      includeTrending: props.includeTrending,
      includePopular: props.includePopular
    };
  }

  componentDidUpdate(prevProps) {
    const {
      includeRecommendations,
      includeTrending,
      includePopular
    } = this.props;

    if (includeRecommendations !== prevProps.includeRecommendations) {
      this.setState({ includeRecommendations });
    }

    if (includeTrending !== prevProps.includeTrending) {
      this.setState({ includeTrending });
    }

    if (includePopular !== prevProps.includePopular) {
      this.setState({ includePopular });
    }
  }

  //
  // Listeners

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
      includeRecommendations,
      includeTrending,
      includePopular
    } = this.state;

    return (
      <>
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
      </>
    );
  }
}

DiscoverMovieTableOptions.propTypes = {
  includeRecommendations: PropTypes.bool.isRequired,
  includeTrending: PropTypes.bool.isRequired,
  includePopular: PropTypes.bool.isRequired,
  onChangeOption: PropTypes.func.isRequired
};

export default DiscoverMovieTableOptions;
