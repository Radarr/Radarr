import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { metadataProfileNames } from 'Helpers/Props';
import createSortedSectionSelector from 'Store/Selectors/createSortedSectionSelector';
import sortByName from 'Utilities/Array/sortByName';
import SelectInput from './SelectInput';

function createMapStateToProps() {
  return createSelector(
    createSortedSectionSelector('settings.metadataProfiles', sortByName),
    (state, { includeNoChange }) => includeNoChange,
    (state, { includeMixed }) => includeMixed,
    (state, { includeNone }) => includeNone,
    (metadataProfiles, includeNoChange, includeMixed, includeNone) => {

      const profiles = metadataProfiles.items.filter((item) => item.name !== metadataProfileNames.NONE);
      const noneProfile = metadataProfiles.items.find((item) => item.name === metadataProfileNames.NONE);

      const values = _.map(profiles, (metadataProfile) => {
        return {
          key: metadataProfile.id,
          value: metadataProfile.name
        };
      });

      if (includeNone) {
        values.push({
          key: noneProfile.id,
          value: noneProfile.name
        });
      }

      if (includeNoChange) {
        values.unshift({
          key: 'noChange',
          value: 'No Change',
          disabled: true
        });
      }

      if (includeMixed) {
        values.unshift({
          key: 'mixed',
          value: '(Mixed)',
          disabled: true
        });
      }

      return {
        values
      };
    }
  );
}

class MetadataProfileSelectInputConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    const {
      name,
      value,
      values
    } = this.props;

    if (!value || !_.some(values, (option) => parseInt(option.key) === value)) {
      const firstValue = _.find(values, (option) => !isNaN(parseInt(option.key)));

      if (firstValue) {
        this.onChange({ name, value: firstValue.key });
      }
    }
  }

  //
  // Listeners

  onChange = ({ name, value }) => {
    this.props.onChange({ name, value: parseInt(value) });
  }

  //
  // Render

  render() {
    return (
      <SelectInput
        {...this.props}
        onChange={this.onChange}
      />
    );
  }
}

MetadataProfileSelectInputConnector.propTypes = {
  name: PropTypes.string.isRequired,
  value: PropTypes.oneOfType([PropTypes.number, PropTypes.string]),
  values: PropTypes.arrayOf(PropTypes.object).isRequired,
  includeNoChange: PropTypes.bool.isRequired,
  includeNone: PropTypes.bool.isRequired,
  onChange: PropTypes.func.isRequired
};

MetadataProfileSelectInputConnector.defaultProps = {
  includeNoChange: false
};

export default connect(createMapStateToProps)(MetadataProfileSelectInputConnector);
