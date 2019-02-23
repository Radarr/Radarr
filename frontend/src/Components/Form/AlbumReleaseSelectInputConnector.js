import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import titleCase from 'Utilities/String/titleCase';
import SelectInput from './SelectInput';

function createMapStateToProps() {
  return createSelector(
    (state, { albumReleases }) => albumReleases,
    (albumReleases) => {
      const values = _.map(albumReleases.value, (albumRelease) => {

        return {
          key: albumRelease.foreignReleaseId,
          value: `${albumRelease.title}` +
            `${albumRelease.disambiguation ? ' (' : ''}${titleCase(albumRelease.disambiguation)}${albumRelease.disambiguation ? ')' : ''}` +
            `, ${albumRelease.mediumCount} med, ${albumRelease.trackCount} tracks` +
            `${albumRelease.country.length > 0 ? ', ' : ''}${albumRelease.country}` +
            `${albumRelease.format ? ', [' : ''}${albumRelease.format}${albumRelease.format ? ']' : ''}`
        };
      });

      const sortedValues = _.orderBy(values, ['value']);

      const value = _.find(albumReleases.value, { monitored: true }).foreignReleaseId;

      return {
        values: sortedValues,
        value
      };
    }
  );
}

class AlbumReleaseSelectInputConnector extends Component {

  //
  // Listeners

  onChange = ({ name, value }) => {
    const {
      albumReleases
    } = this.props;

    const updatedReleases = _.map(albumReleases.value, (e) => ({ ...e, monitored: false }));
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

AlbumReleaseSelectInputConnector.propTypes = {
  name: PropTypes.string.isRequired,
  onChange: PropTypes.func.isRequired,
  albumReleases: PropTypes.object
};

export default connect(createMapStateToProps)(AlbumReleaseSelectInputConnector);
