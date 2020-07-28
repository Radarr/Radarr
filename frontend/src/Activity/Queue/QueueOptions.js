import PropTypes from 'prop-types';
import React, { Component, Fragment } from 'react';
import FormGroup from 'Components/Form/FormGroup';
import FormInputGroup from 'Components/Form/FormInputGroup';
import FormLabel from 'Components/Form/FormLabel';
import { inputTypes } from 'Helpers/Props';

class QueueOptions extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      includeUnknownMovieItems: props.includeUnknownMovieItems
    };
  }

  componentDidUpdate(prevProps) {
    const {
      includeUnknownMovieItems
    } = this.props;

    if (includeUnknownMovieItems !== prevProps.includeUnknownMovieItems) {
      this.setState({
        includeUnknownMovieItems
      });
    }
  }

  //
  // Listeners

  onOptionChange = ({ name, value }) => {
    this.setState({
      [name]: value
    }, () => {
      this.props.onOptionChange({
        [name]: value
      });
    });
  }

  //
  // Render

  render() {
    const {
      includeUnknownMovieItems
    } = this.state;

    return (
      <Fragment>
        <FormGroup>
          <FormLabel>Show Unknown Movie Items</FormLabel>

          <FormInputGroup
            type={inputTypes.CHECK}
            name="includeUnknownMovieItems"
            value={includeUnknownMovieItems}
            helpText="Show items without a movie in the queue, this could include removed movie, movies or anything else in Radarr's category"
            onChange={this.onOptionChange}
          />
        </FormGroup>
      </Fragment>
    );
  }
}

QueueOptions.propTypes = {
  includeUnknownMovieItems: PropTypes.bool.isRequired,
  onOptionChange: PropTypes.func.isRequired
};

export default QueueOptions;
