import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import titleCase from 'Utilities/String/titleCase';
import SelectInput from './SelectInput';

function createMapStateToProps() {
  return createSelector(
    (state, { bookReleases }) => bookReleases,
    (bookReleases) => {
      const values = _.map(bookReleases.value, (bookRelease) => {

        return {
          key: bookRelease.foreignReleaseId,
          value: `${bookRelease.title}` +
            `${bookRelease.disambiguation ? ' (' : ''}${titleCase(bookRelease.disambiguation)}${bookRelease.disambiguation ? ')' : ''}` +
            `, ${bookRelease.mediumCount} med, ${bookRelease.bookCount} books` +
            `${bookRelease.country.length > 0 ? ', ' : ''}${bookRelease.country}` +
            `${bookRelease.format ? ', [' : ''}${bookRelease.format}${bookRelease.format ? ']' : ''}`
        };
      });

      const sortedValues = _.orderBy(values, ['value']);

      const value = _.find(bookReleases.value, { monitored: true }).foreignReleaseId;

      return {
        values: sortedValues,
        value
      };
    }
  );
}

class BookReleaseSelectInputConnector extends Component {

  //
  // Listeners

  onChange = ({ name, value }) => {
    const {
      bookReleases
    } = this.props;

    const updatedReleases = _.map(bookReleases.value, (e) => ({ ...e, monitored: false }));
    _.find(updatedReleases, { foreignReleaseId: value }).monitored = true;

    this.props.onChange({ name, value: updatedReleases });
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

BookReleaseSelectInputConnector.propTypes = {
  name: PropTypes.string.isRequired,
  onChange: PropTypes.func.isRequired,
  bookReleases: PropTypes.object
};

export default connect(createMapStateToProps)(BookReleaseSelectInputConnector);
