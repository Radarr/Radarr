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
      includeRecommendations: props.includeRecommendations
    };
  }

  componentDidUpdate(prevProps) {
    const { includeRecommendations } = this.props;

    if (includeRecommendations !== prevProps.includeRecommendations) {
      this.setState({
        includeRecommendations
      });
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
      includeRecommendations
    } = this.state;

    return (
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
    );
  }
}

DiscoverMovieTableOptions.propTypes = {
  includeRecommendations: PropTypes.bool.isRequired,
  onChangeOption: PropTypes.func.isRequired
};

export default DiscoverMovieTableOptions;
