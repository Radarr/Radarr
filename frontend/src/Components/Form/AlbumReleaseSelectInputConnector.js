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
    (state, { selectedRelease }) => selectedRelease,
    (albumReleases, selectedRelease) => {
      const values = _.map(albumReleases.value, (albumRelease) => {

        return {
          key: albumRelease.id,
          value: `${albumRelease.mediaCount} med, ${albumRelease.trackCount} tracks` +
            `${albumRelease.country.length > 0 ? ', ' : ''}${albumRelease.country}` +
            `${albumRelease.disambiguation ? ', ' : ''}${titleCase(albumRelease.disambiguation)}` +
            `${albumRelease.format ? ', [' : ''}${albumRelease.format}${albumRelease.format ? ']' : ''}`
        };
      });

      const value = selectedRelease.value.id;

      return {
        values,
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

    this.props.onChange({ name, value: _.find(albumReleases.value, { id: value }) });
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
