import PropTypes from 'prop-types';
import React, { Component } from 'react';
import { connect } from 'react-redux';
import { createSelector } from 'reselect';
import { deleteRootFolder } from 'Store/Actions/rootFolderActions';
import ImportMovieRootFolderRow from './ImportMovieRootFolderRow';

function createMapStateToProps() {
  return createSelector(
    () => {
      return {
      };
    }
  );
}

const mapDispatchToProps = {
  deleteRootFolder
};

class ImportMovieRootFolderRowConnector extends Component {

  //
  // Listeners

  onDeletePress = () => {
    this.props.deleteRootFolder({ id: this.props.id });
  };

  //
  // Render

  render() {
    return (
      <ImportMovieRootFolderRow
        {...this.props}
        onDeletePress={this.onDeletePress}
      />
    );
  }
}

ImportMovieRootFolderRowConnector.propTypes = {
  id: PropTypes.number.isRequired,
  deleteRootFolder: PropTypes.func.isRequired
};

export default connect(createMapStateToProps, mapDispatchToProps)(ImportMovieRootFolderRowConnector);
