import PropTypes from 'prop-types';
import React, { Component, Fragment } from 'react';
import { inputTypes } from 'Helpers/Props';
import FormGroup from 'Components/Form/FormGroup';
import FormLabel from 'Components/Form/FormLabel';
import FormInputGroup from 'Components/Form/FormInputGroup';

class QueueOptions extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      includeUnknownArtistItems: props.includeUnknownArtistItems
    };
  }

  componentDidUpdate(prevProps) {
    const {
      includeUnknownArtistItems
    } = this.props;

    if (includeUnknownArtistItems !== prevProps.includeUnknownArtistItems) {
      this.setState({
        includeUnknownArtistItems
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
      includeUnknownArtistItems
    } = this.state;

    return (
      <Fragment>
        <FormGroup>
          <FormLabel>Show Unknown Artist Items</FormLabel>

          <FormInputGroup
            type={inputTypes.CHECK}
            name="includeUnknownArtistItems"
            value={includeUnknownArtistItems}
            helpText="Show items without a artist in the queue, this could include removed artists, movies or anything else in Lidarr's category"
            onChange={this.onOptionChange}
          />
        </FormGroup>
      </Fragment>
    );
  }
}

QueueOptions.propTypes = {
  includeUnknownArtistItems: PropTypes.bool.isRequired,
  onOptionChange: PropTypes.func.isRequired
};

export default QueueOptions;
