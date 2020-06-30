import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import titleCase from 'Utilities/String/titleCase';
import SelectInput from './SelectInput';

function createMapStateToProps() {
  return createSelector(
    (state, { bookEditions }) => bookEditions,
    (bookEditions) => {
      const values = _.map(bookEditions.value, (bookEdition) => {

        let value = `${bookEdition.title}`;

        if (bookEdition.disambiguation) {
          value = `${value} (${titleCase(bookEdition.disambiguation)})`;
        }

        const extras = [];
        if (bookEdition.language) {
          extras.push(bookEdition.language);
        }
        if (bookEdition.publisher) {
          extras.push(bookEdition.publisher);
        }
        if (bookEdition.isbn13) {
          extras.push(bookEdition.isbn13);
        }
        if (bookEdition.format) {
          extras.push(bookEdition.format);
        }
        if (bookEdition.pageCount > 0) {
          extras.push(`${bookEdition.pageCount}p`);
        }

        if (extras) {
          value = `${value} [${extras.join(', ')}]`;
        }

        return {
          key: bookEdition.foreignEditionId,
          value
        };
      });

      const sortedValues = _.orderBy(values, ['value']);

      const value = _.find(bookEditions.value, { monitored: true }).foreignEditionId;

      return {
        values: sortedValues,
        value
      };
    }
  );
}

class BookEditionSelectInputConnector extends Component {

  //
  // Listeners

  onChange = ({ name, value }) => {
    const {
      bookEditions
    } = this.props;

    const updatedEditions = _.map(bookEditions.value, (e) => ({ ...e, monitored: false }));
    _.find(updatedEditions, { foreignEditionId: value }).monitored = true;

    this.props.onChange({ name, value: updatedEditions });
  }

  render() {

    return (
      <SelectInput
        {...this.props}
        onChange={this.onChange}
      />
    );
  }
}

BookEditionSelectInputConnector.propTypes = {
  name: PropTypes.string.isRequired,
  onChange: PropTypes.func.isRequired,
  bookEditions: PropTypes.object
};

export default connect(createMapStateToProps)(BookEditionSelectInputConnector);
