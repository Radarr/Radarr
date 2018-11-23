import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { inputTypes } from 'Helpers/Props';
import FormGroup from 'Components/Form/FormGroup';
import FormLabel from 'Components/Form/FormLabel';
import FormInputGroup from 'Components/Form/FormInputGroup';

class MovieIndexTableOptions extends Component {

  //
  // Lifecycle

  constructor(props, context) {
    super(props, context);

    this.state = {
      showSearchAction: props.showSearchAction
    };
  }

  componentDidUpdate(prevProps) {
    const { showSearchAction } = this.props;

    if (showSearchAction !== prevProps.showSearchAction) {
      this.setState({
        showSearchAction
      });
    }
  }

  //
  // Listeners

  onTableOptionChange = ({ name, value }) => {
    this.setState({
      [name]: value
    }, () => {
      this.props.onTableOptionChange({
        tableOptions: {
          ...this.state,
          [name]: value
        }
      });
    });
  }

  //
  // Render

  render() {
    const {
      showSearchAction
    } = this.state;

    return (
      <FormGroup>
        <FormLabel>Show Search</FormLabel>

        <FormInputGroup
          type={inputTypes.CHECK}
          name="showSearchAction"
          value={showSearchAction}
          helpText="Show search button on hover"
          onChange={this.onTableOptionChange}
        />
      </FormGroup>
    );
  }
}

MovieIndexTableOptions.propTypes = {
  showSearchAction: PropTypes.bool.isRequired,
  onTableOptionChange: PropTypes.func.isRequired
};

export default MovieIndexTableOptions;
