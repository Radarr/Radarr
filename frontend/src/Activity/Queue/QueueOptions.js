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
      includeUnknownAuthorItems: props.includeUnknownAuthorItems
    };
  }

  componentDidUpdate(prevProps) {
    const {
      includeUnknownAuthorItems
    } = this.props;

    if (includeUnknownAuthorItems !== prevProps.includeUnknownAuthorItems) {
      this.setState({
        includeUnknownAuthorItems
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
      includeUnknownAuthorItems
    } = this.state;

    return (
      <Fragment>
        <FormGroup>
          <FormLabel>Show Unknown Author Items</FormLabel>

          <FormInputGroup
            type={inputTypes.CHECK}
            name="includeUnknownAuthorItems"
            value={includeUnknownAuthorItems}
            helpText="Show items without a author in the queue, this could include removed authors, movies or anything else in Readarr's category"
            onChange={this.onOptionChange}
          />
        </FormGroup>
      </Fragment>
    );
  }
}

QueueOptions.propTypes = {
  includeUnknownAuthorItems: PropTypes.bool.isRequired,
  onOptionChange: PropTypes.func.isRequired
};

export default QueueOptions;
