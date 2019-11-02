import _ from 'lodash';
import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { fetchNetImportSchema, selectNetImportSchema } from 'Store/Actions/settingsActions';
import AddNetImportModalContent from './AddNetImportModalContent';

function createMapStateToProps() {
  return createSelector(
    (state) => state.settings.netImports,
    (netImports) => {
      const {
        isSchemaFetching,
        isSchemaPopulated,
        schemaError,
        schema
      } = netImports;

      const listGroups = _.chain(schema)
        .sortBy((o) => o.listOrder)
        .groupBy('listType')
        .value();

      return {
        isSchemaFetching,
        isSchemaPopulated,
        schemaError,
        listGroups
      };
    }
  );
}

const mapDispatchToProps = {
  fetchNetImportSchema,
  selectNetImportSchema
};

class AddNetImportModalContentConnector extends Component {

  //
  // Lifecycle

  componentDidMount() {
    this.props.fetchNetImportSchema();
  }

  //
  // Listeners

  onNetImportSelect = ({ implementation, name }) => {
    this.props.selectNetImportSchema({ implementation, presetName: name });
    this.props.onModalClose({ netImportSelected: true });
  }

  //
  // Render

  render() {
    return (
      <AddNetImportModalContent
        {...this.props}
        onNetImportSelect={this.onNetImportSelect}
      />
    );
  }
}

AddNetImportModalContentConnector.propTypes = {
  fetchNetImportSchema: PropTypes.func.isRequired,
  selectNetImportSchema: PropTypes.func.isRequired,
  onModalClose: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(AddNetImportModalContentConnector);
