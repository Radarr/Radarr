import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import EnhancedSelectInput from './EnhancedSelectInput';

function createMapStateToProps() {
  return createSelector(
    (state, { indexerFlags }) => indexerFlags,
    (state) => state.settings.indexerFlags,
    (selectedFlags, indexerFlags) => {
      const value = [];

      indexerFlags.items.forEach((item) => {
        // eslint-disable-next-line no-bitwise
        if ((selectedFlags & item.id) === item.id) {
          value.push(item.id);
        }
      });

      const values = indexerFlags.items.map(({ id, name }) => {
        return {
          key: id,
          value: name
        };
      });

      return {
        value,
        values
      };
    }
  );
}

class IndexerFlagsSelectInputConnector extends Component {

  onChange = ({ name, value }) => {
    let indexerFlags = 0;

    value.forEach((flagId) => {
      indexerFlags += flagId;
    });

    this.props.onChange({ name, value: indexerFlags });
  };

  //
  // Render

  render() {

    return (
      <EnhancedSelectInput
        {...this.props}
        onChange={this.onChange}
      />
    );
  }
}

IndexerFlagsSelectInputConnector.propTypes = {
  name: PropTypes.string.isRequired,
  indexerFlags: PropTypes.number.isRequired,
  value: PropTypes.arrayOf(PropTypes.number).isRequired,
  values: PropTypes.arrayOf(PropTypes.object).isRequired,
  onChange: PropTypes.func.isRequired
};

export default connect(createMapStateToProps)(IndexerFlagsSelectInputConnector);
